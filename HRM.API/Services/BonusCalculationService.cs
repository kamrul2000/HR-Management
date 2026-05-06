using System.Globalization;
using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Bonus;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class BonusCalculationService : IBonusCalculationService
{
    private const int MaxBulkBatchSize = 500;
    private const int MaxPageSize = 100;

    private static readonly string[] ValidBonusTypes =
        { "Festival", "Performance", "Annual", "Discretionary" };

    private static readonly string[] ValidBases =
        { "FixedAmount", "PercentageOfBasic", "PercentageOfGross" };

    private readonly IRepository<BonusCalculation> _bonusRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<SalaryCalculation> _salaryCalculationRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly ISalaryCreateService _salaryCreateService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public BonusCalculationService(
        IRepository<BonusCalculation> bonusRepository,
        IRepository<Employee> employeeRepository,
        IRepository<SalaryCalculation> salaryCalculationRepository,
        IRepository<Branch> branchRepository,
        ISalaryCreateService salaryCreateService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _bonusRepository = bonusRepository;
        _employeeRepository = employeeRepository;
        _salaryCalculationRepository = salaryCalculationRepository;
        _branchRepository = branchRepository;
        _salaryCreateService = salaryCreateService;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<BonusResponseDto> CreateAsync(CreateBonusDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);

        ValidateBonusType(dto.BonusType);
        ValidateCalculationBasis(dto.CalculationBasis, dto.BasisPercentage, dto.FixedAmount);

        var (basicSnapshot, grossSnapshot) =
            await SnapshotSalaryAsync(employee.Id);

        var computed = ComputeBonusAmount(
            dto.CalculationBasis, dto.BasisPercentage, dto.FixedAmount,
            basicSnapshot, grossSnapshot);

        var now = DateTime.UtcNow;
        var bonus = new BonusCalculation
        {
            EmployeeId = employee.Id,
            BonusType = dto.BonusType,
            BonusTitle = dto.BonusTitle.Trim(),
            CalculationBasis = dto.CalculationBasis,
            BasisPercentage = dto.BasisPercentage,
            BasicSalarySnapshot = basicSnapshot,
            GrossSalarySnapshot = grossSnapshot,
            ComputedAmount = computed,
            FinalAmount = computed,
            DisbursementMonth = dto.DisbursementMonth,
            DisbursementYear = dto.DisbursementYear,
            IsDisbursedWithSalary = dto.IsDisbursedWithSalary,
            Status = "Draft",
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _bonusRepository.AddAsync(bonus);

        return await LoadResponseAsync(bonus.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkCreateAsync(BulkCreateBonusDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var result = new BulkCreateResultDto();

        if (dto.EmployeeIds is null || dto.EmployeeIds.Count == 0)
        {
            throw new InvalidOperationException("At least one employee ID must be provided.");
        }

        if (dto.EmployeeIds.Count > MaxBulkBatchSize)
        {
            throw new InvalidOperationException(
                $"Bulk bonus creation cannot exceed {MaxBulkBatchSize} employees per request.");
        }

        ValidateBonusType(dto.BonusType);
        ValidateCalculationBasis(dto.CalculationBasis, dto.BasisPercentage, dto.FixedAmount);

        foreach (var employeeId in dto.EmployeeIds.Distinct())
        {
            try
            {
                var employee = await ResolveEmployeeAsync(employeeId, subscriptionId);
                var (basicSnapshot, grossSnapshot) = await SnapshotSalaryAsync(employee.Id);

                var computed = ComputeBonusAmount(
                    dto.CalculationBasis, dto.BasisPercentage, dto.FixedAmount,
                    basicSnapshot, grossSnapshot);

                var now = DateTime.UtcNow;
                var bonus = new BonusCalculation
                {
                    EmployeeId = employee.Id,
                    BonusType = dto.BonusType,
                    BonusTitle = dto.BonusTitle.Trim(),
                    CalculationBasis = dto.CalculationBasis,
                    BasisPercentage = dto.BasisPercentage,
                    BasicSalarySnapshot = basicSnapshot,
                    GrossSalarySnapshot = grossSnapshot,
                    ComputedAmount = computed,
                    FinalAmount = computed,
                    DisbursementMonth = dto.DisbursementMonth,
                    DisbursementYear = dto.DisbursementYear,
                    IsDisbursedWithSalary = dto.IsDisbursedWithSalary,
                    Status = "Draft",
                    Remarks = dto.Remarks,
                    SubscriptionId = subscriptionId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _bonusRepository.AddAsync(bonus);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.FailedReasons.Add($"Employee {employeeId}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<BonusResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<BonusResponseDto>> GetFilteredAsync(BonusFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(b => b.EmployeeId == empId);
        }

        if (filter.BranchId is int brId)
        {
            query = query.Where(b => b.Employee.BranchId == brId);
        }

        if (!string.IsNullOrWhiteSpace(filter.BonusType))
        {
            var type = filter.BonusType.Trim();
            query = query.Where(b => b.BonusType == type);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(b => b.Status == status);
        }

        if (filter.DisbursementMonth is int month)
        {
            query = query.Where(b => b.DisbursementMonth == month);
        }

        if (filter.DisbursementYear is int year)
        {
            query = query.Where(b => b.DisbursementYear == year);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<BonusResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<BonusResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(b => b.EmployeeId == employeeId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<BonusSummaryReportDto> GetSummaryReportAsync(int year, int month, int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();

        string? branchName = null;
        if (branchId is int brId)
        {
            var branch = await _branchRepository.GetByIdAsync(brId)
                ?? throw new KeyNotFoundException($"Branch with ID {brId} not found.");

            if (branch.SubscriptionId != subscriptionId)
            {
                throw new UnauthorizedAccessException("Access denied to this branch.");
            }

            branchName = branch.Name;
        }

        var query = BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(b => b.DisbursementYear == year && b.DisbursementMonth == month);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.Employee.BranchId == branchId.Value);
        }

        var bonuses = await query.ToListAsync();

        var approvedOrDisbursed = bonuses
            .Where(b => b.Status == "Approved" || b.Status == "Disbursed")
            .ToList();

        var amountByType = approvedOrDisbursed
            .GroupBy(b => b.BonusType)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.FinalAmount));

        var totalFinalAmount = approvedOrDisbursed.Sum(b => b.FinalAmount);

        return new BonusSummaryReportDto
        {
            DisbursementYear = year,
            DisbursementMonth = month,
            MonthLabel = BuildMonthLabel(year, month),
            BranchId = branchId,
            BranchName = branchName,
            TotalEmployees = bonuses.Select(b => b.EmployeeId).Distinct().Count(),
            ApprovedCount = bonuses.Count(b => b.Status == "Approved"),
            DisbursedCount = bonuses.Count(b => b.Status == "Disbursed"),
            TotalComputedAmount = bonuses.Sum(b => b.ComputedAmount),
            TotalFinalAmount = totalFinalAmount,
            TotalFinalAmountFormatted = totalFinalAmount.ToString("N2"),
            AmountByBonusType = amountByType
        };
    }

    public async Task<BonusResponseDto> ApproveAsync(int id, ApproveBonusDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var bonus = await _bonusRepository.Query()
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Bonus with ID {id} not found.");

        if (bonus.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this bonus.");
        }

        if (bonus.Status != "Draft")
        {
            throw new InvalidOperationException(
                $"Only draft bonuses can be approved. Current status: '{bonus.Status}'.");
        }

        var finalAmount = dto.FinalAmount ?? bonus.ComputedAmount;
        if (finalAmount <= 0)
        {
            throw new InvalidOperationException("FinalAmount must be greater than zero.");
        }

        var now = DateTime.UtcNow;
        bonus.Status = "Approved";
        bonus.FinalAmount = finalAmount;
        bonus.ApprovedById = callerId;
        bonus.ApprovalDate = now;
        bonus.ApprovalRemarks = dto.ApprovalRemarks;
        bonus.UpdatedAt = now;

        await _bonusRepository.UpdateAsync(bonus);

        return await LoadResponseAsync(bonus.Id, subscriptionId);
    }

    public async Task<BonusResponseDto> DisburseAsync(int id, DisburseBonusDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var bonus = await _bonusRepository.Query()
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Bonus with ID {id} not found.");

        if (bonus.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this bonus.");
        }

        if (bonus.Status != "Approved")
        {
            throw new InvalidOperationException(
                $"Only approved bonuses can be disbursed. Current status: '{bonus.Status}'.");
        }

        var now = DateTime.UtcNow;

        if (bonus.IsDisbursedWithSalary)
        {
            if (!dto.SalaryCalculationId.HasValue)
            {
                throw new InvalidOperationException(
                    "SalaryCalculationId is required when IsDisbursedWithSalary is true.");
            }

            var salaryCalc = await _salaryCalculationRepository.Query()
                .FirstOrDefaultAsync(c => c.Id == dto.SalaryCalculationId.Value)
                ?? throw new KeyNotFoundException(
                    $"Salary calculation with ID {dto.SalaryCalculationId.Value} not found.");

            if (salaryCalc.SubscriptionId != subscriptionId)
            {
                throw new UnauthorizedAccessException("Access denied to the linked salary calculation.");
            }

            if (salaryCalc.EmployeeId != bonus.EmployeeId)
            {
                throw new InvalidOperationException(
                    "The salary calculation does not belong to the same employee as the bonus.");
            }

            if (salaryCalc.Status == "Cancelled")
            {
                throw new InvalidOperationException(
                    "Cannot link a bonus to a cancelled salary calculation.");
            }

            salaryCalc.BonusAmount += bonus.FinalAmount;
            salaryCalc.NetSalary += bonus.FinalAmount;
            salaryCalc.UpdatedAt = now;

            bonus.SalaryCalculationId = salaryCalc.Id;
        }

        bonus.Status = "Disbursed";
        bonus.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(bonus.Id, subscriptionId);
    }

    public async Task<BonusResponseDto> CancelAsync(int id, string reason)
    {
        var subscriptionId = GetSubscriptionId();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        var bonus = await _bonusRepository.Query()
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Bonus with ID {id} not found.");

        if (bonus.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this bonus.");
        }

        if (bonus.Status == "Disbursed" || bonus.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                $"Bonus in status '{bonus.Status}' cannot be cancelled.");
        }

        var now = DateTime.UtcNow;
        bonus.Status = "Cancelled";
        bonus.Remarks = string.IsNullOrWhiteSpace(bonus.Remarks)
            ? reason
            : $"{bonus.Remarks} | Cancelled: {reason}";
        bonus.UpdatedAt = now;

        await _bonusRepository.UpdateAsync(bonus);

        return await LoadResponseAsync(bonus.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var bonus = await _bonusRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Bonus with ID {id} not found.");

        if (bonus.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this bonus.");
        }

        if (bonus.Status != "Draft" && bonus.Status != "Cancelled")
        {
            throw new InvalidOperationException(
                $"Only draft or cancelled bonuses can be deleted. Current status: '{bonus.Status}'.");
        }

        await _bonusRepository.DeleteAsync(bonus);
    }

    private IQueryable<BonusCalculation> BaseQuery(int subscriptionId)
    {
        return _bonusRepository
            .Query()
            .Include(b => b.Employee)
            .Where(b => b.SubscriptionId == subscriptionId);
    }

    private async Task<BonusResponseDto> LoadResponseAsync(int bonusId, int subscriptionId)
    {
        var bonus = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bonusId);

        if (bonus is null)
        {
            var existsForOtherTenant = await _bonusRepository.Query()
                .AnyAsync(b => b.Id == bonusId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this bonus.");
            }

            throw new KeyNotFoundException($"Bonus with ID {bonusId} not found.");
        }

        return MapToResponseDto(bonus);
    }

    private BonusResponseDto MapToResponseDto(BonusCalculation b)
    {
        var dto = _mapper.Map<BonusResponseDto>(b);

        dto.BonusTypeLabel = b.BonusType switch
        {
            "Festival" => "Festival Bonus",
            "Performance" => "Performance Bonus",
            "Annual" => "Annual Bonus",
            "Discretionary" => "Discretionary Bonus",
            _ => b.BonusType
        };

        dto.CalculationBasisLabel = b.CalculationBasis switch
        {
            "FixedAmount" => "Fixed Amount",
            "PercentageOfBasic" => "% of Basic Salary",
            "PercentageOfGross" => "% of Gross Salary",
            _ => b.CalculationBasis
        };

        dto.ComputedAmountFormatted = b.ComputedAmount.ToString("N2");
        dto.FinalAmountFormatted = b.FinalAmount.ToString("N2");
        dto.DisbursementPeriodLabel = BuildMonthLabel(b.DisbursementYear, b.DisbursementMonth);
        dto.ApprovalDateFormatted = b.ApprovalDate?.ToString("dd MMM yyyy");

        dto.StatusLabel = b.Status switch
        {
            "Draft" => "Draft",
            "Approved" => "Approved",
            "Disbursed" => "Disbursed",
            "Cancelled" => "Cancelled",
            _ => b.Status
        };

        return dto;
    }

    private async Task<(decimal basicSnapshot, decimal grossSnapshot)> SnapshotSalaryAsync(int employeeId)
    {
        try
        {
            var structure = await _salaryCreateService.GetActiveByEmployeeAsync(employeeId);
            return (structure.BasicSalary, structure.EstimatedGrossSalary);
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException(
                "No active salary structure found for this employee. " +
                "A salary structure must be created before calculating bonus.");
        }
    }

    private static decimal ComputeBonusAmount(
        string basis, decimal? percentage, decimal? fixedAmount,
        decimal basicSnapshot, decimal grossSnapshot)
    {
        decimal computed = basis switch
        {
            "FixedAmount" => fixedAmount!.Value,
            "PercentageOfBasic" => Math.Round(basicSnapshot * percentage!.Value / 100m, 2),
            "PercentageOfGross" => Math.Round(grossSnapshot * percentage!.Value / 100m, 2),
            _ => throw new InvalidOperationException($"Unknown basis: {basis}")
        };

        if (computed <= 0)
        {
            throw new InvalidOperationException(
                $"Computed bonus amount ({computed}) must be greater than zero.");
        }

        return computed;
    }

    private static void ValidateBonusType(string bonusType)
    {
        if (!ValidBonusTypes.Contains(bonusType))
        {
            throw new InvalidOperationException(
                $"Invalid BonusType '{bonusType}'. Accepted: {string.Join(", ", ValidBonusTypes)}.");
        }
    }

    private static void ValidateCalculationBasis(string basis, decimal? percentage, decimal? fixedAmount)
    {
        if (!ValidBases.Contains(basis))
        {
            throw new InvalidOperationException(
                $"Invalid CalculationBasis '{basis}'. Accepted: {string.Join(", ", ValidBases)}.");
        }

        if (basis == "FixedAmount")
        {
            if (!fixedAmount.HasValue || fixedAmount.Value <= 0)
            {
                throw new InvalidOperationException(
                    "FixedAmount must be provided and > 0 when CalculationBasis is 'FixedAmount'.");
            }
            if (percentage.HasValue)
            {
                throw new InvalidOperationException(
                    "BasisPercentage must not be provided when CalculationBasis is 'FixedAmount'.");
            }
        }
        else
        {
            if (!percentage.HasValue)
            {
                throw new InvalidOperationException(
                    $"BasisPercentage is required for CalculationBasis '{basis}'.");
            }
            if (fixedAmount.HasValue)
            {
                throw new InvalidOperationException(
                    "FixedAmount must not be provided when CalculationBasis is percentage-based.");
            }
        }
    }

    private async Task<Employee> ResolveEmployeeAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        if (!employee.IsActive || employee.Status != "Active")
        {
            throw new InvalidOperationException(
                "Bonus can only be created for active employees.");
        }

        return employee;
    }

    private async Task EnsureEmployeeOwnershipAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }
    }

    private static string BuildMonthLabel(int year, int month)
    {
        return $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}";
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }

    private int GetCallerId()
    {
        return _httpContextAccessor.HttpContext?.User.GetUserId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

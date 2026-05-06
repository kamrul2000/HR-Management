using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.GratuityCalculation;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class GratuityCalculationService : IGratuityCalculationService
{
    private readonly IRepository<GratuityCalculation> _calculationRepository;
    private readonly IRepository<GratuityRule> _ruleRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly ISalaryCreateService _salaryCreateService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public GratuityCalculationService(
        IRepository<GratuityCalculation> calculationRepository,
        IRepository<GratuityRule> ruleRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Branch> branchRepository,
        ISalaryCreateService salaryCreateService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _calculationRepository = calculationRepository;
        _ruleRepository = ruleRepository;
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _salaryCreateService = salaryCreateService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<GratuityCalculationResponseDto> ComputeAsync(ComputeGratuityDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {dto.EmployeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        await EnsureNoDuplicateAsync(employee.Id, subscriptionId);

        var separationDate = dto.SeparationDate.Date;
        if (separationDate < employee.JoiningDate.Date)
        {
            throw new InvalidOperationException(
                "Separation date cannot be before the joining date.");
        }

        GratuityRule rule;
        if (dto.GratuityRuleId.HasValue)
        {
            rule = await _ruleRepository.Query()
                .FirstOrDefaultAsync(r => r.Id == dto.GratuityRuleId.Value)
                ?? throw new KeyNotFoundException(
                    $"Gratuity rule with ID {dto.GratuityRuleId.Value} not found.");

            if (rule.SubscriptionId != subscriptionId)
            {
                throw new UnauthorizedAccessException("Access denied to this gratuity rule.");
            }
        }
        else
        {
            rule = await _ruleRepository.Query()
                .FirstOrDefaultAsync(r => r.IsActive && r.SubscriptionId == subscriptionId)
                ?? throw new KeyNotFoundException("No active gratuity rule configured.");
        }

        decimal monthlySalary = await ResolveMonthlySalaryAsync(employee, rule, subscriptionId);

        int workingDaysPerMonth = _configuration.GetValue<int>("GratuitySettings:WorkingDaysPerMonth", 26);
        if (workingDaysPerMonth < 1) workingDaysPerMonth = 26;

        var preview = GratuityRuleService.ComputeGratuity(
            employee, rule, separationDate, monthlySalary, workingDaysPerMonth);

        int totalServiceDays = (int)Math.Floor((separationDate - employee.JoiningDate.Date).TotalDays);
        if (totalServiceDays < 0) totalServiceDays = 0;

        var now = DateTime.UtcNow;
        var calculation = new GratuityCalculation
        {
            EmployeeId = employee.Id,
            GratuityRuleId = rule.Id,
            SeparationDate = separationDate,
            JoiningDate = employee.JoiningDate.Date,
            TotalServiceDays = totalServiceDays,
            TotalServiceYears = preview.TotalServiceYears,
            EligibleYears = preview.EligibleYears,
            CalculationBasis = rule.CalculationBasis,
            MonthlySalaryUsed = preview.MonthlySalaryUsed,
            DailySalary = preview.DailySalary,
            RatePerYear = preview.RatePerYear,
            GratuityBeforeCap = preview.GratuityBeforeCap,
            GratuityAmount = preview.GratuityAmount,
            IsCapApplied = preview.IsCapApplied,
            IsEligible = preview.IsEligible,
            IneligibilityReason = string.IsNullOrWhiteSpace(preview.IneligibilityReason)
                ? null : preview.IneligibilityReason,
            SeparationId = null,
            Status = "Draft",
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.GratuityCalculations.AddAsync(calculation);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(calculation.Id, subscriptionId);
    }

    public async Task<GratuityCalculationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<GratuityCalculationResponseDto> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var calculation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(c => c.EmployeeId == employeeId && c.Status != "Cancelled")
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("No gratuity calculation found for this employee.");

        return MapToResponseDto(calculation);
    }

    public async Task<IEnumerable<GratuityCalculationResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<GratuityReportDto> GetReportAsync(int? branchId = null, string? status = null)
    {
        var subscriptionId = GetSubscriptionId();

        string? branchName = null;
        if (branchId.HasValue)
        {
            var branch = await _branchRepository.GetByIdAsync(branchId.Value)
                ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

            if (branch.SubscriptionId != subscriptionId)
            {
                throw new UnauthorizedAccessException("Access denied to this branch.");
            }

            branchName = branch.Name;
        }

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (branchId.HasValue)
        {
            query = query.Where(c => c.Employee.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var trimmedStatus = status.Trim();
            query = query.Where(c => c.Status == trimmedStatus);
        }

        var records = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var details = records.Select(MapToResponseDto).ToList();

        decimal totalGratuityAmount = details
            .Where(d => d.IsEligible && d.Status != "Cancelled")
            .Sum(d => d.GratuityAmount);

        return new GratuityReportDto
        {
            BranchId = branchId,
            BranchName = branchName,
            TotalRecords = details.Count,
            EligibleCount = details.Count(d => d.IsEligible),
            IneligibleCount = details.Count(d => !d.IsEligible),
            TotalGratuityAmount = totalGratuityAmount,
            TotalGratuityAmountFormatted = totalGratuityAmount.ToString("N2"),
            Details = details
        };
    }

    public async Task<GratuityCalculationResponseDto> FinalizeAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var calculation = await _calculationRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Gratuity calculation with ID {id} not found.");

        if (calculation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this gratuity calculation.");
        }

        if (calculation.Status != "Draft")
        {
            throw new InvalidOperationException(
                $"Only Draft gratuity calculations can be finalized. Current status: '{calculation.Status}'.");
        }

        var now = DateTime.UtcNow;
        calculation.Status = "Finalized";
        calculation.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(calculation.Id, subscriptionId);
    }

    public async Task<GratuityCalculationResponseDto> CancelAsync(int id, string reason)
    {
        var subscriptionId = GetSubscriptionId();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        var calculation = await _calculationRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Gratuity calculation with ID {id} not found.");

        if (calculation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this gratuity calculation.");
        }

        if (calculation.Status == "Cancelled")
        {
            throw new InvalidOperationException("This gratuity calculation is already cancelled.");
        }

        if (calculation.Status == "Finalized" && calculation.SeparationId.HasValue)
        {
            throw new InvalidOperationException(
                "Cannot cancel a finalized gratuity that has been linked to a processed separation.");
        }

        var now = DateTime.UtcNow;
        calculation.Status = "Cancelled";
        calculation.Remarks = string.IsNullOrWhiteSpace(calculation.Remarks)
            ? reason
            : $"{calculation.Remarks} | Cancelled: {reason}";
        calculation.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(calculation.Id, subscriptionId);
    }

    private async Task<decimal> ResolveMonthlySalaryAsync(Employee employee, GratuityRule rule, int subscriptionId)
    {
        if (rule.CalculationBasis == "LastBasicSalary")
        {
            var active = await _salaryCreateService
                .GetActiveByEmployeeInternalAsync(employee.Id, subscriptionId)
                ?? throw new InvalidOperationException(
                    $"No active salary structure found for employee '{employee.FullName}'. " +
                    "A salary structure is required to compute gratuity.");

            return active.BasicSalary;
        }

        var history = (await _salaryCreateService.GetHistoryByEmployeeAsync(employee.Id)).ToList();
        if (history.Count == 0)
        {
            throw new InvalidOperationException(
                $"No salary history found for employee '{employee.FullName}'.");
        }

        decimal totalWeightedSalary = 0m;
        int totalMonths = 0;

        var ordered = history.OrderBy(h => h.EffectiveFrom).ToList();
        var today = DateTime.UtcNow.Date;

        foreach (var revision in ordered)
        {
            var from = revision.EffectiveFrom.Date;
            var to = revision.EffectiveTo?.Date ?? today;

            int months = ((to.Year - from.Year) * 12) + to.Month - from.Month + 1;
            if (months < 1) months = 1;

            totalWeightedSalary += revision.BasicSalary * months;
            totalMonths += months;
        }

        return totalMonths > 0
            ? Math.Round(totalWeightedSalary / totalMonths, 2)
            : ordered.Last().BasicSalary;
    }

    private async Task EnsureNoDuplicateAsync(int employeeId, int subscriptionId, int? excludeId = null)
    {
        var existing = await _calculationRepository.Query()
            .AnyAsync(c =>
                c.EmployeeId == employeeId &&
                c.SubscriptionId == subscriptionId &&
                c.Status != "Cancelled" &&
                (excludeId == null || c.Id != excludeId));

        if (existing)
        {
            throw new InvalidOperationException(
                "An active or finalized gratuity calculation already exists for this employee. " +
                "Cancel the existing record before recomputing.");
        }
    }

    private IQueryable<GratuityCalculation> BaseQuery(int subscriptionId)
    {
        return _calculationRepository.Query()
            .Include(c => c.Employee)
            .Include(c => c.GratuityRule)
            .Where(c => c.SubscriptionId == subscriptionId);
    }

    private async Task<GratuityCalculationResponseDto> LoadResponseAsync(int id, int subscriptionId)
    {
        var calculation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (calculation is null)
        {
            var existsForOtherTenant = await _calculationRepository.Query()
                .AnyAsync(c => c.Id == id);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this gratuity calculation.");
            }

            throw new KeyNotFoundException($"Gratuity calculation with ID {id} not found.");
        }

        return MapToResponseDto(calculation);
    }

    private GratuityCalculationResponseDto MapToResponseDto(GratuityCalculation c)
    {
        var dto = _mapper.Map<GratuityCalculationResponseDto>(c);

        dto.EmployeeCode = c.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = c.Employee?.FullName ?? string.Empty;
        dto.RuleName = c.GratuityRule?.RuleName ?? string.Empty;
        dto.SeparationDateFormatted = c.SeparationDate.ToString("dd MMM yyyy");
        dto.JoiningDateFormatted = c.JoiningDate.ToString("dd MMM yyyy");
        dto.ServicePeriodLabel = FormatServicePeriod(c.TotalServiceYears);

        dto.CalculationBasisLabel = c.CalculationBasis == "LastBasicSalary"
            ? "Last Basic Salary"
            : "Average Basic Salary";

        dto.MonthlySalaryFormatted = c.MonthlySalaryUsed.ToString("N2");
        dto.DailySalaryFormatted = c.DailySalary.ToString("N2");
        dto.GratuityBeforeCapFormatted = c.GratuityBeforeCap.ToString("N2");
        dto.GratuityAmountFormatted = c.GratuityAmount.ToString("N2");

        dto.StatusLabel = c.Status switch
        {
            "Draft" => "Draft",
            "Finalized" => "Finalized",
            "Cancelled" => "Cancelled",
            _ => c.Status
        };

        return dto;
    }

    private static string FormatServicePeriod(decimal totalYears)
    {
        if (totalYears <= 0)
        {
            return "Less than 1 day";
        }

        int years = (int)totalYears;
        decimal remainingYears = totalYears - years;
        int months = (int)(remainingYears * 12);
        decimal remainingMonths = remainingYears * 12 - months;
        int days = (int)(remainingMonths * 30);

        var parts = new List<string>();
        if (years > 0) parts.Add($"{years} year{(years != 1 ? "s" : "")}");
        if (months > 0) parts.Add($"{months} month{(months != 1 ? "s" : "")}");
        if (days > 0) parts.Add($"{days} day{(days != 1 ? "s" : "")}");

        return parts.Count > 0 ? string.Join(" ", parts) : "Less than 1 day";
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

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

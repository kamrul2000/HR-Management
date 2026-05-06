using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.PfInterest;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class PfInterestService : IPfInterestService
{
    private readonly IRepository<PfInterestRate> _rateRepository;
    private readonly IRepository<EmployeePfInterest> _interestRepository;
    private readonly IRepository<EmployeePfContribution> _contributionRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public PfInterestService(
        IRepository<PfInterestRate> rateRepository,
        IRepository<EmployeePfInterest> interestRepository,
        IRepository<EmployeePfContribution> contributionRepository,
        IRepository<Employee> employeeRepository,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _rateRepository = rateRepository;
        _interestRepository = interestRepository;
        _contributionRepository = contributionRepository;
        _employeeRepository = employeeRepository;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<PfInterestRateResponseDto> CreateRateAsync(CreatePfInterestRateDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var trimmedFy = dto.FiscalYear.Trim();
        ValidateFiscalYearFormat(trimmedFy);

        var duplicate = await _rateRepository.Query()
            .AnyAsync(r => r.FiscalYear == trimmedFy && r.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"An interest rate for fiscal year '{trimmedFy}' already exists.");
        }

        var now = DateTime.UtcNow;
        var rate = new PfInterestRate
        {
            FiscalYear = trimmedFy,
            InterestRate = dto.InterestRate,
            EffectiveFrom = dto.EffectiveFrom.Date,
            IsActive = true,
            Description = dto.Description,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.PfInterestRates.AddAsync(rate);
        await _context.SaveChangesAsync();

        return MapRateToDto(rate);
    }

    public async Task<IEnumerable<PfInterestRateResponseDto>> GetAllRatesAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var rates = await _rateRepository.Query()
            .AsNoTracking()
            .Where(r => r.SubscriptionId == subscriptionId)
            .OrderByDescending(r => r.FiscalYear)
            .ToListAsync();

        return rates.Select(MapRateToDto).ToList();
    }

    public async Task<PfInterestRateResponseDto> GetRateByFiscalYearAsync(string fiscalYear)
    {
        var subscriptionId = GetSubscriptionId();
        var trimmedFy = fiscalYear?.Trim() ?? string.Empty;

        var rate = await _rateRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.FiscalYear == trimmedFy && r.SubscriptionId == subscriptionId)
            ?? throw new KeyNotFoundException(
                $"Interest rate for fiscal year '{trimmedFy}' not found.");

        return MapRateToDto(rate);
    }

    public async Task<EmployeePfInterestResponseDto> ComputeAsync(ComputePfInterestDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var trimmedFy = dto.FiscalYear.Trim();
        ValidateFiscalYearFormat(trimmedFy);

        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);
        var rate = await GetActiveRateEntityAsync(trimmedFy, subscriptionId);

        var existing = await _interestRepository.Query()
            .AnyAsync(r =>
                r.EmployeeId == employee.Id &&
                r.FiscalYear == trimmedFy &&
                r.SubscriptionId == subscriptionId);

        if (existing)
        {
            throw new InvalidOperationException(
                $"PF interest for employee '{employee.FullName}' in fiscal year '{trimmedFy}' has already been computed.");
        }

        var record = await ComputeInterestRecordAsync(employee.Id, trimmedFy, rate, subscriptionId);

        await _context.EmployeePfInterests.AddAsync(record);
        await _context.SaveChangesAsync();

        return await LoadInterestResponseAsync(record.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkComputeAsync(BulkComputePfInterestDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var trimmedFy = dto.FiscalYear.Trim();
        ValidateFiscalYearFormat(trimmedFy);

        var rate = await GetActiveRateEntityAsync(trimmedFy, subscriptionId);

        var employeeQuery = _employeeRepository.Query()
            .Where(e => e.SubscriptionId == subscriptionId && e.IsActive && e.Status == "Active");

        if (dto.BranchId.HasValue)
        {
            employeeQuery = employeeQuery.Where(e => e.BranchId == dto.BranchId.Value);
        }

        var employees = await employeeQuery.AsNoTracking().ToListAsync();

        var result = new BulkCreateResultDto();

        foreach (var employee in employees)
        {
            try
            {
                var existing = await _interestRepository.Query()
                    .AnyAsync(r =>
                        r.EmployeeId == employee.Id &&
                        r.FiscalYear == trimmedFy &&
                        r.SubscriptionId == subscriptionId);

                if (existing)
                {
                    result.SkippedCount++;
                    result.SkippedReasons.Add(
                        $"{employee.EmployeeCode}: PF interest for '{trimmedFy}' already computed.");
                    continue;
                }

                var record = await ComputeInterestRecordAsync(employee.Id, trimmedFy, rate, subscriptionId);
                await _context.EmployeePfInterests.AddAsync(record);
                await _context.SaveChangesAsync();
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.FailedReasons.Add($"{employee.EmployeeCode}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<EmployeePfInterestResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadInterestResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<EmployeePfInterestResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var records = await _interestRepository.Query()
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.PfInterestRate)
            .Where(r => r.EmployeeId == employeeId && r.SubscriptionId == subscriptionId)
            .OrderByDescending(r => r.FiscalYear)
            .ToListAsync();

        return records.Select(MapInterestToDto).ToList();
    }

    public async Task<PfInterestReportDto> GetReportAsync(string fiscalYear, int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();
        var trimmedFy = fiscalYear?.Trim() ?? string.Empty;
        ValidateFiscalYearFormat(trimmedFy);

        var rate = await _rateRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.FiscalYear == trimmedFy && r.SubscriptionId == subscriptionId)
            ?? throw new KeyNotFoundException(
                $"Interest rate for fiscal year '{trimmedFy}' not found.");

        var query = _interestRepository.Query()
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.PfInterestRate)
            .Where(r => r.FiscalYear == trimmedFy && r.SubscriptionId == subscriptionId);

        if (branchId.HasValue)
        {
            query = query.Where(r => r.Employee.BranchId == branchId.Value);
        }

        var records = await query
            .OrderBy(r => r.Employee.FullName)
            .ToListAsync();

        var details = records.Select(MapInterestToDto).ToList();

        decimal totalOpening = details.Sum(d => d.OpeningBalance);
        decimal totalContrib = details.Sum(d => d.TotalContributionsForYear);
        decimal totalInterest = details.Sum(d => d.InterestAmount);
        decimal totalClosing = details.Sum(d => d.ClosingBalance);

        return new PfInterestReportDto
        {
            FiscalYear = trimmedFy,
            InterestRate = rate.InterestRate,
            InterestRateLabel = $"{rate.InterestRate:F2}%",
            TotalEmployees = details.Count,
            TotalOpeningBalance = totalOpening,
            TotalOpeningBalanceFormatted = totalOpening.ToString("N2"),
            TotalContributions = totalContrib,
            TotalContributionsFormatted = totalContrib.ToString("N2"),
            TotalInterestCredited = totalInterest,
            TotalInterestCreditedFormatted = totalInterest.ToString("N2"),
            TotalClosingBalance = totalClosing,
            TotalClosingBalanceFormatted = totalClosing.ToString("N2"),
            Details = details
        };
    }

    private async Task<EmployeePfInterest> ComputeInterestRecordAsync(
        int employeeId, string fiscalYear, PfInterestRate rate, int subscriptionId)
    {
        var (fyStart, fyEnd) = ParseFiscalYear(fiscalYear);

        decimal openingBalance = await GetOpeningBalanceAsync(employeeId, fiscalYear, subscriptionId);

        var contributions = await _contributionRepository.Query()
            .AsNoTracking()
            .Where(c =>
                c.EmployeeId == employeeId &&
                c.SubscriptionId == subscriptionId)
            .Select(c => new { c.Year, c.Month, c.TotalContribution })
            .ToListAsync();

        decimal totalContributions = contributions
            .Where(c =>
            {
                var period = new DateTime(c.Year, c.Month, 1);
                return period >= fyStart && period <= fyEnd;
            })
            .Sum(c => c.TotalContribution);

        decimal avgBalance = openingBalance + (totalContributions / 2m);
        decimal interest = Math.Round(avgBalance * rate.InterestRate / 100m, 2);
        decimal closing = openingBalance + totalContributions + interest;

        var now = DateTime.UtcNow;
        return new EmployeePfInterest
        {
            EmployeeId = employeeId,
            PfInterestRateId = rate.Id,
            FiscalYear = fiscalYear,
            OpeningBalance = Math.Round(openingBalance, 2),
            TotalContributionsForYear = Math.Round(totalContributions, 2),
            AverageBalance = Math.Round(avgBalance, 2),
            InterestRate = rate.InterestRate,
            InterestAmount = interest,
            ClosingBalance = Math.Round(closing, 2),
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private async Task<decimal> GetOpeningBalanceAsync(
        int employeeId, string fiscalYear, int subscriptionId)
    {
        var previous = await _interestRepository.Query()
            .AsNoTracking()
            .Where(r =>
                r.EmployeeId == employeeId &&
                r.SubscriptionId == subscriptionId &&
                string.Compare(r.FiscalYear, fiscalYear) < 0)
            .OrderByDescending(r => r.FiscalYear)
            .FirstOrDefaultAsync();

        return previous?.ClosingBalance ?? 0m;
    }

    private (DateTime start, DateTime end) ParseFiscalYear(string fiscalYear)
    {
        var parts = fiscalYear.Split('-');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int startYear) ||
            !int.TryParse(parts[1], out int endYear))
        {
            throw new InvalidOperationException(
                $"Invalid fiscal year format '{fiscalYear}'. Expected format: 'YYYY-YYYY' e.g. '2024-2025'.");
        }

        int startMonth = _configuration.GetValue<int>("FiscalYearSettings:StartMonth", 1);
        int endMonth = _configuration.GetValue<int>("FiscalYearSettings:EndMonth", 12);

        if (startMonth < 1 || startMonth > 12) startMonth = 1;
        if (endMonth < 1 || endMonth > 12) endMonth = 12;

        DateTime start;
        DateTime end;

        if (startMonth == 1 && endMonth == 12)
        {
            start = new DateTime(startYear, 1, 1);
            end = new DateTime(endYear, 12, 31);
        }
        else
        {
            start = new DateTime(startYear, startMonth, 1);
            end = new DateTime(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth));
        }

        return (start, end);
    }

    private static void ValidateFiscalYearFormat(string fiscalYear)
    {
        if (string.IsNullOrWhiteSpace(fiscalYear))
        {
            throw new InvalidOperationException("FiscalYear is required.");
        }

        var parts = fiscalYear.Split('-');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int startYear) ||
            !int.TryParse(parts[1], out int endYear))
        {
            throw new InvalidOperationException(
                $"Invalid fiscal year format '{fiscalYear}'. Expected format: 'YYYY-YYYY' e.g. '2024-2025'.");
        }

        if (startYear < 2000 || endYear > 2100 || endYear < startYear)
        {
            throw new InvalidOperationException(
                $"Invalid fiscal year range '{fiscalYear}'.");
        }
    }

    private async Task<PfInterestRate> GetActiveRateEntityAsync(string fiscalYear, int subscriptionId)
    {
        var rate = await _rateRepository.Query()
            .FirstOrDefaultAsync(r => r.FiscalYear == fiscalYear && r.SubscriptionId == subscriptionId)
            ?? throw new KeyNotFoundException(
                $"Interest rate for fiscal year '{fiscalYear}' not found.");

        if (!rate.IsActive)
        {
            throw new InvalidOperationException(
                $"The interest rate for fiscal year '{fiscalYear}' is inactive.");
        }

        return rate;
    }

    private async Task<EmployeePfInterestResponseDto> LoadInterestResponseAsync(int id, int subscriptionId)
    {
        var record = await _interestRepository.Query()
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.PfInterestRate)
            .FirstOrDefaultAsync(r => r.Id == id && r.SubscriptionId == subscriptionId);

        if (record is null)
        {
            var existsForOtherTenant = await _interestRepository.Query()
                .AnyAsync(r => r.Id == id);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this PF interest record.");
            }

            throw new KeyNotFoundException($"PF interest record with ID {id} not found.");
        }

        return MapInterestToDto(record);
    }

    private PfInterestRateResponseDto MapRateToDto(PfInterestRate r)
    {
        var dto = _mapper.Map<PfInterestRateResponseDto>(r);
        dto.InterestRateLabel = $"{r.InterestRate:F2}% per annum";
        dto.EffectiveFromFormatted = r.EffectiveFrom.ToString("dd MMM yyyy");
        return dto;
    }

    private EmployeePfInterestResponseDto MapInterestToDto(EmployeePfInterest r)
    {
        var dto = _mapper.Map<EmployeePfInterestResponseDto>(r);

        dto.EmployeeCode = r.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = r.Employee?.FullName ?? string.Empty;
        dto.InterestRateLabel = $"{r.InterestRate:F2}%";
        dto.OpeningBalanceFormatted = r.OpeningBalance.ToString("N2");
        dto.TotalContributionsFormatted = r.TotalContributionsForYear.ToString("N2");
        dto.AverageBalanceFormatted = r.AverageBalance.ToString("N2");
        dto.InterestAmountFormatted = r.InterestAmount.ToString("N2");
        dto.ClosingBalanceFormatted = r.ClosingBalance.ToString("N2");

        return dto;
    }

    private async Task<Employee> ResolveEmployeeAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
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

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

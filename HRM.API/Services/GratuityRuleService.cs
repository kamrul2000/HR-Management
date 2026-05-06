using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.GratuitySetup;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class GratuityRuleService : IGratuityRuleService
{
    private readonly IRepository<GratuityRule> _ruleRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly ISalaryCreateService _salaryCreateService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public GratuityRuleService(
        IRepository<GratuityRule> ruleRepository,
        IRepository<Employee> employeeRepository,
        ISalaryCreateService salaryCreateService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _ruleRepository = ruleRepository;
        _employeeRepository = employeeRepository;
        _salaryCreateService = salaryCreateService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<GratuityRuleResponseDto> CreateAsync(CreateGratuityRuleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateCalculationBasis(dto.CalculationBasis);

        var trimmedName = dto.RuleName.Trim();
        var duplicate = await _ruleRepository.Query()
            .AnyAsync(r => r.RuleName == trimmedName && r.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"A gratuity rule named '{trimmedName}' already exists.");
        }

        var now = DateTime.UtcNow;

        var existingActive = await _ruleRepository.Query()
            .Where(r => r.SubscriptionId == subscriptionId && r.IsActive)
            .ToListAsync();

        foreach (var existing in existingActive)
        {
            existing.IsActive = false;
            existing.UpdatedAt = now;
        }

        var rule = new GratuityRule
        {
            RuleName = trimmedName,
            MinServiceYears = dto.MinServiceYears,
            CalculationBasis = dto.CalculationBasis,
            RatePerYear = dto.RatePerYear,
            MaxGratuityAmount = dto.MaxGratuityAmount,
            MaxServiceYearsCapped = dto.MaxServiceYearsCapped,
            ProRataEnabled = dto.ProRataEnabled,
            IsActive = true,
            EffectiveFrom = dto.EffectiveFrom.Date,
            Description = dto.Description,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.GratuityRules.AddAsync(rule);
        await _context.SaveChangesAsync();

        return MapToResponseDto(rule);
    }

    public async Task<GratuityRuleResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        var rule = await LoadRuleAsync(id, subscriptionId);
        return MapToResponseDto(rule);
    }

    public async Task<GratuityRuleResponseDto> GetActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var rule = await _ruleRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IsActive && r.SubscriptionId == subscriptionId)
            ?? throw new KeyNotFoundException("No active gratuity rule configured.");

        return MapToResponseDto(rule);
    }

    public async Task<IEnumerable<GratuityRuleResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var rules = await _ruleRepository.Query()
            .AsNoTracking()
            .Where(r => r.SubscriptionId == subscriptionId)
            .OrderByDescending(r => r.IsActive)
            .ThenByDescending(r => r.EffectiveFrom)
            .ToListAsync();

        return rules.Select(MapToResponseDto).ToList();
    }

    public async Task<GratuityRuleResponseDto> UpdateAsync(int id, UpdateGratuityRuleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateCalculationBasis(dto.CalculationBasis);

        var rule = await LoadRuleAsync(id, subscriptionId);

        var trimmedName = dto.RuleName.Trim();
        var duplicate = await _ruleRepository.Query()
            .AnyAsync(r =>
                r.Id != id &&
                r.SubscriptionId == subscriptionId &&
                r.RuleName == trimmedName);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"A gratuity rule named '{trimmedName}' already exists.");
        }

        var now = DateTime.UtcNow;

        if (dto.IsActive && !rule.IsActive)
        {
            var others = await _ruleRepository.Query()
                .Where(r =>
                    r.Id != id &&
                    r.SubscriptionId == subscriptionId &&
                    r.IsActive)
                .ToListAsync();

            foreach (var other in others)
            {
                other.IsActive = false;
                other.UpdatedAt = now;
            }
        }

        rule.RuleName = trimmedName;
        rule.MinServiceYears = dto.MinServiceYears;
        rule.CalculationBasis = dto.CalculationBasis;
        rule.RatePerYear = dto.RatePerYear;
        rule.MaxGratuityAmount = dto.MaxGratuityAmount;
        rule.MaxServiceYearsCapped = dto.MaxServiceYearsCapped;
        rule.ProRataEnabled = dto.ProRataEnabled;
        rule.IsActive = dto.IsActive;
        rule.Description = dto.Description;
        rule.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return MapToResponseDto(rule);
    }

    public async Task<GratuityPreviewResultDto> PreviewAsync(GratuityPreviewDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {dto.EmployeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        if (dto.SeparationDate.Date < employee.JoiningDate.Date)
        {
            throw new InvalidOperationException(
                "Separation date cannot be before the employee's joining date.");
        }

        GratuityRule rule;
        if (dto.GratuityRuleId.HasValue)
        {
            rule = await LoadRuleAsync(dto.GratuityRuleId.Value, subscriptionId);
        }
        else
        {
            rule = await _ruleRepository.Query()
                .FirstOrDefaultAsync(r => r.IsActive && r.SubscriptionId == subscriptionId)
                ?? throw new KeyNotFoundException("No active gratuity rule configured.");
        }

        decimal monthlySalary = await ResolveMonthlySalaryAsync(employee, rule);

        int workingDaysPerMonth = _configuration.GetValue<int>("GratuitySettings:WorkingDaysPerMonth", 26);
        if (workingDaysPerMonth < 1) workingDaysPerMonth = 26;

        return ComputeGratuity(employee, rule, dto.SeparationDate.Date, monthlySalary, workingDaysPerMonth);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        var rule = await LoadRuleAsync(id, subscriptionId);

        // Module 29 (GratuityCalculation) reference check will be added when that module exists.
        // For now there is no FK constraint preventing deletion.

        await _ruleRepository.DeleteAsync(rule);
    }

    private async Task<decimal> ResolveMonthlySalaryAsync(Employee employee, GratuityRule rule)
    {
        if (rule.CalculationBasis == "LastBasicSalary")
        {
            var active = await _salaryCreateService
                .GetActiveByEmployeeInternalAsync(employee.Id, employee.SubscriptionId)
                ?? throw new InvalidOperationException(
                    $"No active salary structure found for employee '{employee.FullName}'.");

            return active.BasicSalary;
        }

        // AverageBasicSalary — simple average of distinct revisions
        var history = await _salaryCreateService.GetHistoryByEmployeeAsync(employee.Id);
        var historyList = history.ToList();

        if (historyList.Count == 0)
        {
            throw new InvalidOperationException(
                $"No salary structure history found for employee '{employee.FullName}'.");
        }

        return Math.Round(historyList.Average(s => s.BasicSalary), 2);
    }

    public static GratuityPreviewResultDto ComputeGratuity(
        Employee employee,
        GratuityRule rule,
        DateTime separationDate,
        decimal monthlySalary,
        int workingDaysPerMonth = 26)
    {
        var result = new GratuityPreviewResultDto
        {
            EmployeeId = employee.Id,
            EmployeeFullName = employee.FullName,
            EmployeeCode = employee.EmployeeCode,
            JoiningDate = employee.JoiningDate,
            JoiningDateFormatted = employee.JoiningDate.ToString("dd MMM yyyy"),
            SeparationDate = separationDate,
            SeparationDateFormatted = separationDate.ToString("dd MMM yyyy"),
            RuleName = rule.RuleName,
            CalculationBasis = rule.CalculationBasis,
            MonthlySalaryUsed = monthlySalary,
            MonthlySalaryFormatted = monthlySalary.ToString("N2"),
            RatePerYear = rule.RatePerYear
        };

        double totalDays = (separationDate.Date - employee.JoiningDate.Date).TotalDays;
        if (totalDays < 0) totalDays = 0;

        decimal totalYears = (decimal)(totalDays / 365.25);
        result.TotalServiceYears = Math.Round(totalYears, 4);
        result.ServicePeriodLabel = FormatServicePeriod(totalYears);

        if (totalYears < rule.MinServiceYears)
        {
            result.IsEligible = false;
            result.IneligibilityReason =
                $"Minimum service of {rule.MinServiceYears} year(s) not met. " +
                $"Actual service: {FormatServicePeriod(totalYears)}.";
            result.GratuityAmount = 0m;
            result.GratuityAmountFormatted = "0.00";
            return result;
        }

        result.IsEligible = true;

        decimal eligibleYears = rule.MaxServiceYearsCapped.HasValue
            ? Math.Min(totalYears, rule.MaxServiceYearsCapped.Value)
            : totalYears;

        if (!rule.ProRataEnabled)
        {
            eligibleYears = Math.Floor(eligibleYears);
        }

        result.EligibleYears = Math.Round(eligibleYears, 4);

        decimal dailySalary = workingDaysPerMonth > 0
            ? Math.Round(monthlySalary / workingDaysPerMonth, 4)
            : 0m;
        result.DailySalary = dailySalary;

        decimal gratuityBeforeCap = Math.Round(eligibleYears * rule.RatePerYear * dailySalary, 2);
        result.GratuityBeforeCap = gratuityBeforeCap;

        decimal finalGratuity = gratuityBeforeCap;
        if (rule.MaxGratuityAmount.HasValue && gratuityBeforeCap > rule.MaxGratuityAmount.Value)
        {
            finalGratuity = rule.MaxGratuityAmount.Value;
            result.IsCapApplied = true;
        }

        result.GratuityAmount = finalGratuity;
        result.GratuityAmountFormatted = finalGratuity.ToString("N2");

        return result;
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

    private static void ValidateCalculationBasis(string basis)
    {
        if (basis != "LastBasicSalary" && basis != "AverageBasicSalary")
        {
            throw new InvalidOperationException(
                "CalculationBasis must be 'LastBasicSalary' or 'AverageBasicSalary'.");
        }
    }

    private async Task<GratuityRule> LoadRuleAsync(int id, int subscriptionId)
    {
        var rule = await _ruleRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rule is null)
        {
            throw new KeyNotFoundException($"Gratuity rule with ID {id} not found.");
        }

        if (rule.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this gratuity rule.");
        }

        return rule;
    }

    private GratuityRuleResponseDto MapToResponseDto(GratuityRule r)
    {
        var dto = _mapper.Map<GratuityRuleResponseDto>(r);

        dto.MinServiceYearsLabel = r.MinServiceYears >= 1m
            ? $"{r.MinServiceYears:G29} year(s)"
            : $"{r.MinServiceYears * 12m:G29} month(s)";

        dto.CalculationBasisLabel = r.CalculationBasis == "LastBasicSalary"
            ? "Last Basic Salary"
            : "Average Basic Salary";

        dto.RatePerYearLabel = $"{r.RatePerYear:G29} days per year of service";

        dto.MaxGratuityAmountFormatted = r.MaxGratuityAmount?.ToString("N2");

        dto.MaxServiceYearsCappedLabel = r.MaxServiceYearsCapped.HasValue
            ? $"Capped at {r.MaxServiceYearsCapped} years"
            : null;

        dto.ProRataLabel = r.ProRataEnabled
            ? "Pro-rata (partial years counted)"
            : "Whole years only";

        dto.EffectiveFromFormatted = r.EffectiveFrom.ToString("dd MMM yyyy");

        return dto;
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

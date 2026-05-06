using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.PfContribution;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class PfContributionService : IPfContributionService
{
    private const int MaxPageSize = 200;

    private readonly IRepository<PfRule> _ruleRepository;
    private readonly IRepository<EmployeePfContribution> _contributionRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly ISalaryCreateService _salaryCreateService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public PfContributionService(
        IRepository<PfRule> ruleRepository,
        IRepository<EmployeePfContribution> contributionRepository,
        IRepository<Employee> employeeRepository,
        ISalaryCreateService salaryCreateService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _ruleRepository = ruleRepository;
        _contributionRepository = contributionRepository;
        _employeeRepository = employeeRepository;
        _salaryCreateService = salaryCreateService;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<PfRuleResponseDto> CreateRuleAsync(CreatePfRuleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        ValidatePfBase(dto.PfBase);

        var trimmedName = dto.RuleName.Trim();
        var duplicate = await RuleBaseQuery(subscriptionId)
            .AnyAsync(r => r.RuleName == trimmedName);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"A PF rule named '{trimmedName}' already exists.");
        }

        var now = DateTime.UtcNow;

        var existingActive = await RuleBaseQuery(subscriptionId)
            .Where(r => r.IsActive)
            .ToListAsync();

        foreach (var existing in existingActive)
        {
            existing.IsActive = false;
            existing.EffectiveTo = dto.EffectiveFrom.Date.AddDays(-1);
            existing.UpdatedAt = now;
        }

        var rule = new PfRule
        {
            RuleName = trimmedName,
            EmployeeContributionRate = dto.EmployeeContributionRate,
            EmployerContributionRate = dto.EmployerContributionRate,
            PfBase = dto.PfBase,
            MinEligibleSalary = dto.MinEligibleSalary,
            MaxContributionAmount = dto.MaxContributionAmount,
            EffectiveFrom = dto.EffectiveFrom.Date,
            EffectiveTo = null,
            IsActive = true,
            Description = dto.Description,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.PfRules.AddAsync(rule);
        await _context.SaveChangesAsync();

        return MapRuleToDto(rule);
    }

    public async Task<PfRuleResponseDto> GetRuleByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        var rule = await LoadRuleAsync(id, subscriptionId);
        return MapRuleToDto(rule);
    }

    public async Task<PfRuleResponseDto> GetActiveRuleAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var rule = await RuleBaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IsActive)
            ?? throw new KeyNotFoundException("No active PF rule configured.");

        return MapRuleToDto(rule);
    }

    public async Task<IEnumerable<PfRuleResponseDto>> GetAllRulesAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var rules = await RuleBaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderByDescending(r => r.IsActive)
            .ThenByDescending(r => r.EffectiveFrom)
            .ToListAsync();

        return rules.Select(MapRuleToDto).ToList();
    }

    public async Task<PfRuleResponseDto> UpdateRuleAsync(int id, UpdatePfRuleDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var rule = await LoadRuleAsync(id, subscriptionId);

        var trimmedName = dto.RuleName.Trim();
        var duplicate = await RuleBaseQuery(subscriptionId)
            .AnyAsync(r => r.Id != id && r.RuleName == trimmedName);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"A PF rule named '{trimmedName}' already exists.");
        }

        var now = DateTime.UtcNow;

        if (dto.IsActive && !rule.IsActive)
        {
            var others = await RuleBaseQuery(subscriptionId)
                .Where(r => r.Id != id && r.IsActive)
                .ToListAsync();

            foreach (var other in others)
            {
                other.IsActive = false;
                other.UpdatedAt = now;
            }
        }

        rule.RuleName = trimmedName;
        rule.EmployeeContributionRate = dto.EmployeeContributionRate;
        rule.EmployerContributionRate = dto.EmployerContributionRate;
        rule.MinEligibleSalary = dto.MinEligibleSalary;
        rule.MaxContributionAmount = dto.MaxContributionAmount;
        rule.EffectiveTo = dto.EffectiveTo?.Date;
        rule.IsActive = dto.IsActive;
        rule.Description = dto.Description;
        rule.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return MapRuleToDto(rule);
    }

    public async Task<EmployeePfContributionResponseDto> ComputeAsync(int employeeId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();

        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException("Year must be between 2000 and 2100.");
        }

        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("Month must be between 1 and 12.");
        }

        var employee = await ResolveEmployeeAsync(employeeId, subscriptionId);
        var rule = await GetActiveRuleEntityAsync(subscriptionId);

        var existing = await _contributionRepository.Query()
            .AnyAsync(c =>
                c.EmployeeId == employeeId &&
                c.Year == year &&
                c.Month == month &&
                c.SubscriptionId == subscriptionId);

        if (existing)
        {
            throw new InvalidOperationException(
                "PF contribution already computed for this employee and period.");
        }

        var pfBase = await ResolvePfBaseAsync(employee, rule, subscriptionId);
        var (empContrib, emplrContrib, total) = ComputePfAmounts(pfBase, rule);

        var now = DateTime.UtcNow;
        var contribution = new EmployeePfContribution
        {
            EmployeeId = employee.Id,
            PfRuleId = rule.Id,
            Year = year,
            Month = month,
            PfBase = pfBase,
            EmployeeContributionRate = rule.EmployeeContributionRate,
            EmployerContributionRate = rule.EmployerContributionRate,
            EmployeeContribution = empContrib,
            EmployerContribution = emplrContrib,
            TotalContribution = total,
            SalaryCalculationId = null,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.EmployeePfContributions.AddAsync(contribution);
        await _context.SaveChangesAsync();

        return await LoadContributionResponseAsync(contribution.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkComputeAsync(int year, int month, int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();

        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException("Year must be between 2000 and 2100.");
        }

        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("Month must be between 1 and 12.");
        }

        var rule = await GetActiveRuleEntityAsync(subscriptionId);

        var employeeQuery = _employeeRepository.Query()
            .Where(e => e.SubscriptionId == subscriptionId && e.IsActive && e.Status == "Active");

        if (branchId.HasValue)
        {
            employeeQuery = employeeQuery.Where(e => e.BranchId == branchId.Value);
        }

        var employees = await employeeQuery.AsNoTracking().ToListAsync();

        var result = new BulkCreateResultDto();
        var now = DateTime.UtcNow;

        foreach (var employee in employees)
        {
            try
            {
                var existing = await _contributionRepository.Query()
                    .AnyAsync(c =>
                        c.EmployeeId == employee.Id &&
                        c.Year == year &&
                        c.Month == month &&
                        c.SubscriptionId == subscriptionId);

                if (existing)
                {
                    result.SkippedCount++;
                    result.SkippedReasons.Add(
                        $"{employee.EmployeeCode}: PF contribution already exists for {year}-{month:00}.");
                    continue;
                }

                var pfBase = await ResolvePfBaseAsync(employee, rule, subscriptionId);
                var (empContrib, emplrContrib, total) = ComputePfAmounts(pfBase, rule);

                var contribution = new EmployeePfContribution
                {
                    EmployeeId = employee.Id,
                    PfRuleId = rule.Id,
                    Year = year,
                    Month = month,
                    PfBase = pfBase,
                    EmployeeContributionRate = rule.EmployeeContributionRate,
                    EmployerContributionRate = rule.EmployerContributionRate,
                    EmployeeContribution = empContrib,
                    EmployerContribution = emplrContrib,
                    TotalContribution = total,
                    SalaryCalculationId = null,
                    SubscriptionId = subscriptionId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _context.EmployeePfContributions.AddAsync(contribution);
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

    public async Task<EmployeePfContributionResponseDto> GetContributionByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadContributionResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<EmployeePfContributionResponseDto>> GetByEmployeeAsync(int employeeId, int? year = null)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var query = ContributionBaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(c => c.EmployeeId == employeeId);

        if (year.HasValue)
        {
            query = query.Where(c => c.Year == year.Value);
        }

        var items = await query
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .ToListAsync();

        return items.Select(MapContributionToDto).ToList();
    }

    public async Task<PagedResultDto<EmployeePfContributionResponseDto>> GetFilteredAsync(PfContributionFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = ContributionBaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(c => c.EmployeeId == empId);
        }

        if (filter.BranchId is int branchId)
        {
            query = query.Where(c => c.Employee.BranchId == branchId);
        }

        if (filter.Year is int y)
        {
            query = query.Where(c => c.Year == y);
        }

        if (filter.Month is int m)
        {
            query = query.Where(c => c.Month == m);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .ThenBy(c => c.Employee.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<EmployeePfContributionResponseDto>
        {
            Items = items.Select(MapContributionToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PfMonthlyReportDto> GetMonthlyReportAsync(int year, int month, int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();

        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("Month must be between 1 and 12.");
        }

        var query = ContributionBaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(c => c.Year == year && c.Month == month);

        if (branchId.HasValue)
        {
            query = query.Where(c => c.Employee.BranchId == branchId.Value);
        }

        var items = await query
            .OrderBy(c => c.Employee.FullName)
            .ToListAsync();

        var contributions = items.Select(MapContributionToDto).ToList();

        decimal totalEmployee = contributions.Sum(c => c.EmployeeContribution);
        decimal totalEmployer = contributions.Sum(c => c.EmployerContribution);
        decimal grandTotal = totalEmployee + totalEmployer;

        return new PfMonthlyReportDto
        {
            Year = year,
            Month = month,
            MonthLabel = new DateTime(year, month, 1).ToString("MMMM yyyy"),
            TotalEmployees = contributions.Count,
            TotalEmployeeContribution = totalEmployee,
            TotalEmployeeContributionFormatted = totalEmployee.ToString("N2"),
            TotalEmployerContribution = totalEmployer,
            TotalEmployerContributionFormatted = totalEmployer.ToString("N2"),
            TotalContribution = grandTotal,
            TotalContributionFormatted = grandTotal.ToString("N2"),
            Contributions = contributions
        };
    }

    public async Task<decimal> GetEmployeeMonthlyPfAsync(int employeeId, int subscriptionId)
    {
        var rule = await _ruleRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IsActive && r.SubscriptionId == subscriptionId);

        if (rule is null) return 0m;

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee is null || employee.SubscriptionId != subscriptionId) return 0m;

        var structure = await _salaryCreateService
            .GetActiveByEmployeeInternalAsync(employeeId, subscriptionId);

        if (structure is null) return 0m;

        decimal pfBase = rule.PfBase == "Basic"
            ? structure.BasicSalary
            : structure.Items
                .Where(i => i.IsProvidentFundApplicable && i.HeadType == "Earning")
                .Sum(i => i.FixedAmount ?? 0m);

        if (pfBase == 0m && rule.PfBase == "PFApplicableHeads")
        {
            pfBase = structure.BasicSalary;
        }

        var (empContrib, _, _) = ComputePfAmounts(pfBase, rule);
        return empContrib;
    }

    private static (decimal employeeContrib, decimal employerContrib, decimal total)
        ComputePfAmounts(decimal pfBase, PfRule rule)
    {
        if (rule.MinEligibleSalary.HasValue && pfBase < rule.MinEligibleSalary.Value)
        {
            return (0m, 0m, 0m);
        }

        decimal empContrib = Math.Round(pfBase * rule.EmployeeContributionRate / 100m, 2);
        decimal emplrContrib = Math.Round(pfBase * rule.EmployerContributionRate / 100m, 2);
        decimal total = empContrib + emplrContrib;

        if (rule.MaxContributionAmount.HasValue && total > rule.MaxContributionAmount.Value)
        {
            decimal scale = rule.MaxContributionAmount.Value / total;
            empContrib = Math.Round(empContrib * scale, 2);
            emplrContrib = Math.Round(emplrContrib * scale, 2);
            total = empContrib + emplrContrib;
        }

        return (empContrib, emplrContrib, total);
    }

    private async Task<decimal> ResolvePfBaseAsync(Employee employee, PfRule rule, int subscriptionId)
    {
        var structure = await _salaryCreateService
            .GetActiveByEmployeeInternalAsync(employee.Id, subscriptionId)
            ?? throw new InvalidOperationException(
                $"No active salary structure for employee '{employee.FullName}'.");

        if (rule.PfBase == "Basic")
        {
            return structure.BasicSalary;
        }

        decimal pfBase = structure.Items
            .Where(i => i.IsProvidentFundApplicable && i.HeadType == "Earning")
            .Sum(i => i.FixedAmount ?? 0m);

        if (pfBase == 0m)
        {
            pfBase = structure.BasicSalary;
        }

        return pfBase;
    }

    private static void ValidatePfBase(string pfBase)
    {
        if (pfBase != "Basic" && pfBase != "PFApplicableHeads")
        {
            throw new InvalidOperationException(
                "PfBase must be 'Basic' or 'PFApplicableHeads'.");
        }
    }

    private IQueryable<PfRule> RuleBaseQuery(int subscriptionId)
    {
        return _ruleRepository
            .Query()
            .Where(r => r.SubscriptionId == subscriptionId);
    }

    private IQueryable<EmployeePfContribution> ContributionBaseQuery(int subscriptionId)
    {
        return _contributionRepository
            .Query()
            .Include(c => c.Employee)
            .Include(c => c.PfRule)
            .Where(c => c.SubscriptionId == subscriptionId);
    }

    private async Task<PfRule> LoadRuleAsync(int id, int subscriptionId)
    {
        var rule = await _ruleRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rule is null)
        {
            throw new KeyNotFoundException($"PF rule with ID {id} not found.");
        }

        if (rule.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this PF rule.");
        }

        return rule;
    }

    private async Task<PfRule> GetActiveRuleEntityAsync(int subscriptionId)
    {
        return await _ruleRepository.Query()
            .FirstOrDefaultAsync(r => r.IsActive && r.SubscriptionId == subscriptionId)
            ?? throw new InvalidOperationException(
                "No active PF rule configured. Create a rule before computing contributions.");
    }

    private async Task<EmployeePfContributionResponseDto> LoadContributionResponseAsync(int id, int subscriptionId)
    {
        var contribution = await ContributionBaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contribution is null)
        {
            var existsForOtherTenant = await _contributionRepository.Query()
                .AnyAsync(c => c.Id == id);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this PF contribution.");
            }

            throw new KeyNotFoundException($"PF contribution with ID {id} not found.");
        }

        return MapContributionToDto(contribution);
    }

    private PfRuleResponseDto MapRuleToDto(PfRule r)
    {
        var dto = _mapper.Map<PfRuleResponseDto>(r);

        dto.MinEligibleSalaryFormatted = r.MinEligibleSalary?.ToString("N2");
        dto.MaxContributionAmountFormatted = r.MaxContributionAmount?.ToString("N2");
        dto.EffectiveFromFormatted = r.EffectiveFrom.ToString("dd MMM yyyy");
        dto.EffectiveToFormatted = r.EffectiveTo?.ToString("dd MMM yyyy");

        return dto;
    }

    private EmployeePfContributionResponseDto MapContributionToDto(EmployeePfContribution c)
    {
        var dto = _mapper.Map<EmployeePfContributionResponseDto>(c);

        dto.EmployeeCode = c.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = c.Employee?.FullName ?? string.Empty;
        dto.RuleName = c.PfRule?.RuleName ?? string.Empty;
        dto.PeriodLabel = new DateTime(c.Year, c.Month, 1).ToString("MMMM yyyy");

        dto.PfBaseFormatted = c.PfBase.ToString("N2");
        dto.EmployeeContributionFormatted = c.EmployeeContribution.ToString("N2");
        dto.EmployerContributionFormatted = c.EmployerContribution.ToString("N2");
        dto.TotalContributionFormatted = c.TotalContribution.ToString("N2");

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

using System.Globalization;
using System.Text.Json;
using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.DTOs.LoanInstallment;
using HRM.Core.DTOs.SalaryCalculation;
using HRM.Core.DTOs.TaxSlab;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class SalaryCalculationService : ISalaryCalculationService
{
    private const int MaxPageSize = 200;
    private const decimal StandardHoursPerDay = 8m;

    private readonly IRepository<SalaryCalculation> _calculationRepository;
    private readonly IRepository<SalaryCalculationDetail> _detailRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IRepository<LeaveApplication> _leaveApplicationRepository;
    private readonly ISalaryCreateService _salaryCreateService;
    private readonly IAttendanceService _attendanceService;
    private readonly IOvertimeService _overtimeService;
    private readonly ILoanInstallmentService _loanInstallmentService;
    private readonly ITaxSlabService _taxSlabService;
    private readonly IWorkingDayCalculator _workingDayCalculator;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public SalaryCalculationService(
        IRepository<SalaryCalculation> calculationRepository,
        IRepository<SalaryCalculationDetail> detailRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Branch> branchRepository,
        IRepository<LeaveApplication> leaveApplicationRepository,
        ISalaryCreateService salaryCreateService,
        IAttendanceService attendanceService,
        IOvertimeService overtimeService,
        ILoanInstallmentService loanInstallmentService,
        ITaxSlabService taxSlabService,
        IWorkingDayCalculator workingDayCalculator,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _calculationRepository = calculationRepository;
        _detailRepository = detailRepository;
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _leaveApplicationRepository = leaveApplicationRepository;
        _salaryCreateService = salaryCreateService;
        _attendanceService = attendanceService;
        _overtimeService = overtimeService;
        _loanInstallmentService = loanInstallmentService;
        _taxSlabService = taxSlabService;
        _workingDayCalculator = workingDayCalculator;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<SalaryCalculationResponseDto> CalculateAsync(RunSalaryCalculationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);

        var existing = await _calculationRepository.Query()
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c =>
                c.EmployeeId == employee.Id &&
                c.Year == dto.Year &&
                c.Month == dto.Month &&
                c.SubscriptionId == subscriptionId);

        if (existing is not null)
        {
            if (existing.Status == "Finalized")
            {
                throw new InvalidOperationException(
                    "A finalized salary calculation exists for this period. Cancel it before recalculating.");
            }

            if (existing.Status == "Draft")
            {
                foreach (var detail in existing.Details.ToList())
                {
                    _context.SalaryCalculationDetails.Remove(detail);
                }
                _context.SalaryCalculations.Remove(existing);
                await _context.SaveChangesAsync();
            }
            // "Cancelled" → fall through and create a new record
        }

        var (calculation, details, snapshot) = await ComputeSalaryAsync(
            employee, dto.Year, dto.Month, subscriptionId, dto.Remarks);

        calculation.CalculationDetails = JsonSerializer.Serialize(snapshot);

        await _context.SalaryCalculations.AddAsync(calculation);
        await _context.SaveChangesAsync();

        foreach (var detail in details)
        {
            detail.SalaryCalculationId = calculation.Id;
            await _context.SalaryCalculationDetails.AddAsync(detail);
        }

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(calculation.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkCalculateAsync(BulkRunSalaryDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var result = new BulkCreateResultDto();

        IEnumerable<int> targetIds;

        if (dto.EmployeeIds is { Count: > 0 })
        {
            targetIds = dto.EmployeeIds.Distinct();
        }
        else
        {
            var query = _employeeRepository.Query()
                .Where(e => e.SubscriptionId == subscriptionId && e.IsActive);

            if (dto.BranchId is int branchId)
            {
                await EnsureBranchOwnershipAsync(branchId, subscriptionId);
                query = query.Where(e => e.BranchId == branchId);
            }

            targetIds = await query.Select(e => e.Id).ToListAsync();
        }

        foreach (var employeeId in targetIds)
        {
            try
            {
                await CalculateAsync(new RunSalaryCalculationDto
                {
                    EmployeeId = employeeId,
                    Year = dto.Year,
                    Month = dto.Month,
                    Remarks = dto.Remarks
                });
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

    public async Task<SalaryCalculationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<SalaryCalculationResponseDto> GetByEmployeeMonthAsync(int employeeId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var calculation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.EmployeeId == employeeId &&
                c.Year == year &&
                c.Month == month)
            ?? throw new KeyNotFoundException(
                $"No salary calculation found for employee {employeeId} in {year}/{month:D2}.");

        return MapToResponseDto(calculation);
    }

    public async Task<PagedResultDto<SalaryCalculationResponseDto>> GetFilteredAsync(
        SalaryCalculationFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(c => c.EmployeeId == empId);
        }

        if (filter.BranchId is int brId)
        {
            query = query.Where(c => c.Employee.BranchId == brId);
        }

        if (filter.DepartmentId is int deptId)
        {
            query = query.Where(c => c.Employee.DepartmentId == deptId);
        }

        if (filter.Year is int year)
        {
            query = query.Where(c => c.Year == year);
        }

        if (filter.Month is int month)
        {
            query = query.Where(c => c.Month == month);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(c => c.Status == status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .ThenBy(c => c.Employee.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<SalaryCalculationResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<MonthlySalaryReportDto> GetMonthlyReportAsync(int year, int month, int? branchId = null)
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
            .Where(c => c.Year == year && c.Month == month);

        if (branchId.HasValue)
        {
            query = query.Where(c => c.Employee.BranchId == branchId.Value);
        }

        var calculations = await query
            .OrderBy(c => c.Employee.FullName)
            .ToListAsync();

        var responses = calculations.Select(MapToResponseDto).ToList();

        return new MonthlySalaryReportDto
        {
            Year = year,
            Month = month,
            MonthLabel = BuildMonthLabel(year, month),
            BranchId = branchId,
            BranchName = branchName,
            TotalEmployees = responses.Count,
            FinalizedCount = responses.Count(r => r.Status == "Finalized"),
            DraftCount = responses.Count(r => r.Status == "Draft"),
            TotalGrossSalary = responses.Sum(r => r.GrossSalary),
            TotalDeductions = responses.Sum(r => r.TotalDeductions),
            TotalOvertimePay = responses.Sum(r => r.OvertimePay),
            TotalNetSalary = responses.Sum(r => r.NetSalary),
            TotalNetSalaryFormatted = responses.Sum(r => r.NetSalary).ToString("N2"),
            Calculations = responses
        };
    }

    public async Task<SalaryCalculationResponseDto> FinalizeAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var calculation = await _calculationRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Salary calculation with ID {id} not found.");

        if (calculation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this salary calculation.");
        }

        if (calculation.Status != "Draft")
        {
            throw new InvalidOperationException(
                $"Only draft calculations can be finalized. Current status: '{calculation.Status}'.");
        }

        var now = DateTime.UtcNow;
        calculation.Status = "Finalized";
        calculation.FinalizedAt = now;
        calculation.UpdatedAt = now;

        await _calculationRepository.UpdateAsync(calculation);

        // Post-finalization hook: pay the loan installment for this period (Module 23 integration)
        var pending = await _loanInstallmentService.GetPendingInstallmentAsync(
            calculation.EmployeeId, calculation.Year, calculation.Month);

        if (pending is not null)
        {
            await _loanInstallmentService.ProcessPaymentAsync(pending.InstallmentId, new ProcessInstallmentDto
            {
                PaidAmount = pending.InstallmentAmount,
                SalaryCalculationId = calculation.Id
            });
        }

        return await LoadResponseAsync(calculation.Id, subscriptionId);
    }

    public async Task<SalaryCalculationResponseDto> CancelAsync(int id, string reason)
    {
        var subscriptionId = GetSubscriptionId();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        var calculation = await _calculationRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Salary calculation with ID {id} not found.");

        if (calculation.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this salary calculation.");
        }

        if (calculation.Status == "Cancelled")
        {
            throw new InvalidOperationException("This salary calculation is already cancelled.");
        }

        var now = DateTime.UtcNow;
        calculation.Status = "Cancelled";
        calculation.CancelledAt = now;
        calculation.Remarks = string.IsNullOrWhiteSpace(calculation.Remarks)
            ? reason
            : $"{calculation.Remarks} | Cancelled: {reason}";
        calculation.UpdatedAt = now;

        await _calculationRepository.UpdateAsync(calculation);

        return await LoadResponseAsync(calculation.Id, subscriptionId);
    }

    private async Task<(SalaryCalculation calculation, List<SalaryCalculationDetail> details, object snapshot)>
        ComputeSalaryAsync(Employee employee, int year, int month, int subscriptionId, string? remarks)
    {
        var policy = ReadSalaryPolicy();

        var firstOfMonth = new DateTime(year, month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        var structure = await _salaryCreateService
            .GetStructureActiveOnDateAsync(employee.Id, firstOfMonth)
            ?? throw new InvalidOperationException(
                $"No active salary structure found for employee '{employee.FullName}' as of {firstOfMonth:dd MMM yyyy}.");

        int totalWorkingDays = await _workingDayCalculator
            .CountWorkingDaysAsync(firstOfMonth, lastOfMonth, employee.BranchId);

        if (totalWorkingDays == 0)
        {
            throw new InvalidOperationException(
                $"Total working days for {year}/{month:D2} is zero. " +
                "Check HolidayCalendar and OffDays configuration.");
        }

        var attendance = await _attendanceService.GetMonthlySummaryAsync(employee.Id, year, month);
        var overtime = await _overtimeService.GetMonthlySummaryAsync(employee.Id, year, month);

        decimal unpaidLeaveDays = await GetUnpaidLeaveDaysAsync(
            employee.Id, year, month, subscriptionId);

        decimal lateDeductionDays = policy.lateAllowance > 0
            ? Math.Floor((decimal)attendance.LateDays / policy.lateAllowance) * policy.lateDeductionPerDay
            : 0m;

        decimal basicSalary = structure.BasicSalary;
        decimal dailyRate = basicSalary / totalWorkingDays;

        var (details, grossSalary) = ComputeHeadAmounts(structure, basicSalary, subscriptionId);

        decimal absentDeductionDays =
            attendance.AbsentDays +
            (attendance.HalfDays * 0.5m) +
            unpaidLeaveDays +
            lateDeductionDays;

        decimal attendanceDeduction = Math.Round(dailyRate * absentDeductionDays, 2);
        decimal totalEarnings = Math.Round(grossSalary - attendanceDeduction, 2);

        decimal hourlyRate = basicSalary / (totalWorkingDays * StandardHoursPerDay);
        decimal overtimePay = Math.Round(
            (overtime.RegularOvertimeMinutes / 60m) * hourlyRate * policy.regularOTMultiplier +
            (overtime.HolidayOvertimeMinutes / 60m) * hourlyRate * policy.holidayOTMultiplier +
            (overtime.WeeklyOffOvertimeMinutes / 60m) * hourlyRate * policy.weeklyOffOTMultiplier,
            2);

        decimal loanDeduction = await GetMonthlyLoanDeductionAsync(
            employee.Id, year, month, subscriptionId);

        decimal taxDeduction = await GetMonthlyTaxDeductionAsync(
            employee.Id, year, month, totalEarnings, subscriptionId);

        decimal headDeductions = details
            .Where(d => d.HeadType == "Deduction")
            .Sum(d => d.ComputedAmount);

        decimal totalDeductions = Math.Round(headDeductions + loanDeduction + taxDeduction, 2);

        decimal netSalary = Math.Round(totalEarnings + overtimePay - totalDeductions, 2);

        string? netRemarks = remarks;
        if (netSalary < 0)
        {
            netSalary = 0m;
            var floor = "[Net salary floored at zero — deductions exceeded earnings.]";
            netRemarks = string.IsNullOrWhiteSpace(remarks) ? floor : $"{remarks} {floor}";
        }

        var now = DateTime.UtcNow;
        var calculation = new SalaryCalculation
        {
            EmployeeId = employee.Id,
            SalaryStructureId = structure.Id,
            Year = year,
            Month = month,
            TotalWorkingDays = totalWorkingDays,
            PresentDays = attendance.PresentDays,
            AbsentDays = attendance.AbsentDays,
            HalfDays = attendance.HalfDays,
            UnpaidLeaveDays = unpaidLeaveDays,
            LateDeductionDays = lateDeductionDays,
            OvertimeMinutes = overtime.TotalApprovedMinutes,
            BasicSalary = basicSalary,
            GrossSalary = Math.Round(grossSalary, 2),
            TotalEarnings = totalEarnings,
            TotalDeductions = totalDeductions,
            OvertimePay = overtimePay,
            BonusAmount = 0m,
            LoanDeduction = loanDeduction,
            TaxDeduction = taxDeduction,
            NetSalary = netSalary,
            Status = "Draft",
            Remarks = netRemarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var snapshot = new
        {
            policy = new
            {
                policy.lateAllowance,
                policy.lateDeductionPerDay,
                policy.regularOTMultiplier,
                policy.holidayOTMultiplier,
                policy.weeklyOffOTMultiplier,
                standardHoursPerDay = StandardHoursPerDay
            },
            workingDays = new { totalWorkingDays, dailyRate, hourlyRate },
            attendanceSummary = attendance,
            overtimeSummary = overtime,
            unpaidLeaveDays,
            lateDeductionDays,
            attendanceDeduction
        };

        return (calculation, details, snapshot);
    }

    private (List<SalaryCalculationDetail> details, decimal grossSalary)
        ComputeHeadAmounts(SalaryStructure structure, decimal basicSalary, int subscriptionId)
    {
        var details = new List<SalaryCalculationDetail>();
        var resolvedValues = new Dictionary<int, decimal>();
        decimal grossSalary = 0m;

        var items = structure.Items
            .Where(i => i.SalaryHead.IsActive)
            .ToList();

        var passes = new (string headType, string method)[]
        {
            ("Earning", "Fixed"),
            ("Earning", "PercentageOfBasic"),
            ("Earning", "PercentageOfHead"),
            ("Earning", "PercentageOfGross"),
            ("Deduction", "Fixed"),
            ("Deduction", "PercentageOfBasic"),
            ("Deduction", "PercentageOfHead"),
            ("Deduction", "PercentageOfGross"),
            ("Deduction", "PercentageOfNet")
        };

        var now = DateTime.UtcNow;

        foreach (var (headType, method) in passes)
        {
            var passItems = items
                .Where(i =>
                    i.SalaryHead.HeadType == headType &&
                    i.SalaryHead.CalculationMethod == method)
                .OrderBy(i => i.SalaryHead.DisplayOrder)
                .ToList();

            foreach (var item in passItems)
            {
                var head = item.SalaryHead;
                decimal effectivePct = item.OverridePercentage ?? head.Percentage ?? 0m;
                decimal baseAmount = 0m;
                decimal computed = 0m;

                switch (method)
                {
                    case "Fixed":
                        computed = item.FixedAmount ?? 0m;
                        break;

                    case "PercentageOfBasic":
                        baseAmount = basicSalary;
                        computed = Math.Round(baseAmount * effectivePct / 100m, 2);
                        break;

                    case "PercentageOfGross":
                        baseAmount = grossSalary;
                        computed = Math.Round(baseAmount * effectivePct / 100m, 2);
                        break;

                    case "PercentageOfHead":
                        if (head.BaseHeadId.HasValue &&
                            resolvedValues.TryGetValue(head.BaseHeadId.Value, out var baseVal))
                        {
                            baseAmount = baseVal;
                            computed = Math.Round(baseAmount * effectivePct / 100m, 2);
                        }
                        break;

                    case "PercentageOfNet":
                        // Approximation: gross at this stage stands in for net
                        baseAmount = grossSalary;
                        computed = Math.Round(baseAmount * effectivePct / 100m, 2);
                        break;
                }

                resolvedValues[head.Id] = computed;

                if (headType == "Earning")
                {
                    grossSalary += computed;
                }

                details.Add(new SalaryCalculationDetail
                {
                    SalaryHeadId = head.Id,
                    HeadName = head.HeadName,
                    HeadCode = head.HeadCode,
                    HeadType = head.HeadType,
                    CalculationMethod = head.CalculationMethod,
                    BaseAmount = baseAmount > 0 ? baseAmount : null,
                    AppliedPercentage = effectivePct > 0 ? effectivePct : null,
                    ComputedAmount = computed,
                    DisplayOrder = head.DisplayOrder,
                    SubscriptionId = subscriptionId,
                    CreatedAt = now
                });
            }
        }

        return (details, grossSalary);
    }

    private IQueryable<SalaryCalculation> BaseQuery(int subscriptionId)
    {
        return _calculationRepository
            .Query()
            .Include(c => c.Employee)
            .Include(c => c.SalaryStructure)
            .Include(c => c.Details)
            .Where(c => c.SubscriptionId == subscriptionId);
    }

    private async Task<SalaryCalculationResponseDto> LoadResponseAsync(int calculationId, int subscriptionId)
    {
        var calculation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == calculationId);

        if (calculation is null)
        {
            var existsForOtherTenant = await _calculationRepository.Query()
                .AnyAsync(c => c.Id == calculationId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this salary calculation.");
            }

            throw new KeyNotFoundException($"Salary calculation with ID {calculationId} not found.");
        }

        return MapToResponseDto(calculation);
    }

    private SalaryCalculationResponseDto MapToResponseDto(SalaryCalculation c)
    {
        var dto = _mapper.Map<SalaryCalculationResponseDto>(c);

        dto.MonthLabel = BuildMonthLabel(c.Year, c.Month);
        dto.OvertimeFormatted = FormatMinutes(c.OvertimeMinutes);
        dto.BasicSalaryFormatted = c.BasicSalary.ToString("N2");
        dto.GrossSalaryFormatted = c.GrossSalary.ToString("N2");

        dto.AttendanceDeduction = Math.Round(c.GrossSalary - c.TotalEarnings, 2);
        dto.AttendanceDeductionFormatted = dto.AttendanceDeduction.ToString("N2");

        dto.TotalEarningsFormatted = c.TotalEarnings.ToString("N2");
        dto.OvertimePayFormatted = c.OvertimePay.ToString("N2");
        dto.TotalDeductionsFormatted = c.TotalDeductions.ToString("N2");
        dto.NetSalaryFormatted = c.NetSalary.ToString("N2");

        dto.StatusLabel = c.Status switch
        {
            "Draft" => "Draft",
            "Finalized" => "Finalized",
            "Cancelled" => "Cancelled",
            _ => c.Status
        };

        var detailDtos = c.Details
            .OrderBy(d => d.HeadType)
            .ThenBy(d => d.DisplayOrder)
            .Select(d => new SalaryCalculationDetailResponseDto
            {
                Id = d.Id,
                SalaryHeadId = d.SalaryHeadId,
                HeadName = d.HeadName,
                HeadCode = d.HeadCode,
                HeadType = d.HeadType,
                HeadTypeLabel = d.HeadType == "Earning" ? "Earning (+)" : "Deduction (-)",
                CalculationMethod = d.CalculationMethod,
                BaseAmount = d.BaseAmount,
                AppliedPercentage = d.AppliedPercentage,
                ComputedAmount = d.ComputedAmount,
                ComputedAmountFormatted = d.ComputedAmount.ToString("N2"),
                DisplayOrder = d.DisplayOrder
            })
            .ToList();

        dto.EarningDetails = detailDtos.Where(d => d.HeadType == "Earning").ToList();
        dto.DeductionDetails = detailDtos.Where(d => d.HeadType == "Deduction").ToList();

        return dto;
    }

    private (int lateAllowance, decimal lateDeductionPerDay,
             decimal regularOTMultiplier, decimal holidayOTMultiplier,
             decimal weeklyOffOTMultiplier) ReadSalaryPolicy()
    {
        return (
            _configuration.GetValue<int>("SalaryPolicy:LateAllowanceCount", 3),
            _configuration.GetValue<decimal>("SalaryPolicy:LateDeductionPerDay", 0.5m),
            _configuration.GetValue<decimal>("SalaryPolicy:RegularOTMultiplier", 1.5m),
            _configuration.GetValue<decimal>("SalaryPolicy:HolidayOTMultiplier", 2.0m),
            _configuration.GetValue<decimal>("SalaryPolicy:WeeklyOffOTMultiplier", 2.0m)
        );
    }

    private async Task<decimal> GetUnpaidLeaveDaysAsync(int employeeId, int year, int month, int subscriptionId)
    {
        // Module 13 integration: sum approved unpaid-type leave days within the month.
        // LeaveApplication.LeaveType.IsPaid == false; status "Approved".
        var firstOfMonth = new DateTime(year, month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        var unpaid = await _leaveApplicationRepository.Query()
            .AsNoTracking()
            .Include(a => a.LeaveType)
            .Where(a =>
                a.EmployeeId == employeeId &&
                a.SubscriptionId == subscriptionId &&
                a.Status == "Approved" &&
                a.LeaveType.IsPaid == false &&
                a.FromDate <= lastOfMonth &&
                a.ToDate >= firstOfMonth)
            .SumAsync(a => (decimal?)a.TotalDays) ?? 0m;

        return unpaid;
    }

    private async Task<decimal> GetMonthlyLoanDeductionAsync(int employeeId, int year, int month, int subscriptionId)
    {
        var pending = await _loanInstallmentService.GetPendingInstallmentAsync(employeeId, year, month);
        return pending?.InstallmentAmount ?? 0m;
    }

    private async Task<decimal> GetMonthlyTaxDeductionAsync(
        int employeeId, int year, int month, decimal taxableIncome, int subscriptionId)
    {
        // TODO Module 25 integration — short-circuit when employee is tax-exempt:
        //   if (await _excludeTaxService.IsExcludedAsync(employeeId, subscriptionId)) return 0m;

        var config = await _taxSlabService.GetActiveConfigAsync(subscriptionId);
        if (config is null)
        {
            return 0m;
        }

        decimal projectedAnnualIncome = taxableIncome * 12m;
        var result = TaxSlabService.ComputeTax(projectedAnnualIncome, config);
        return result.MonthlyTax;
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
                "Salary can only be calculated for active employees.");
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

    private async Task EnsureBranchOwnershipAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
        }
    }

    private static string BuildMonthLabel(int year, int month)
    {
        return $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}";
    }

    private static string FormatMinutes(int totalMinutes)
    {
        int h = totalMinutes / 60;
        int m = totalMinutes % 60;
        return m > 0 ? $"{h}h {m}m" : $"{h}h";
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

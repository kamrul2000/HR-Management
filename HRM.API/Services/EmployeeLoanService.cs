using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.EmployeeLoan;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class EmployeeLoanService : IEmployeeLoanService
{
    private const int MaxPageSize = 100;

    private readonly IRepository<EmployeeLoan> _loanRepository;
    private readonly IRepository<LoanInstallment> _installmentRepository;
    private readonly IRepository<LoanApplication> _applicationRepository;
    private readonly IRepository<LoanApproval> _approvalRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public EmployeeLoanService(
        IRepository<EmployeeLoan> loanRepository,
        IRepository<LoanInstallment> installmentRepository,
        IRepository<LoanApplication> applicationRepository,
        IRepository<LoanApproval> approvalRepository,
        IRepository<Employee> employeeRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _loanRepository = loanRepository;
        _installmentRepository = installmentRepository;
        _applicationRepository = applicationRepository;
        _approvalRepository = approvalRepository;
        _employeeRepository = employeeRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<EmployeeLoanResponseDto> CreateAsync(CreateEmployeeLoanDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var application = await _applicationRepository.Query()
            .Include(la => la.Employee)
            .FirstOrDefaultAsync(la => la.Id == dto.LoanApplicationId)
            ?? throw new KeyNotFoundException($"Loan application with ID {dto.LoanApplicationId} not found.");

        if (application.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        if (application.Status != "Approved")
        {
            throw new InvalidOperationException(
                $"Cannot disburse a loan with application status '{application.Status}'. " +
                "Only 'Approved' applications can be disbursed.");
        }

        var approval = await _approvalRepository.Query()
            .FirstOrDefaultAsync(a =>
                a.LoanApplicationId == dto.LoanApplicationId &&
                a.SubscriptionId == subscriptionId &&
                a.Decision == "Approved")
            ?? throw new KeyNotFoundException(
                "No approved LoanApproval record found for this loan application.");

        if (!approval.ApprovedAmount.HasValue ||
            !approval.ApprovedTenureMonths.HasValue ||
            !approval.MonthlyInstallment.HasValue)
        {
            throw new InvalidOperationException(
                "Loan approval record is missing required financial fields.");
        }

        var duplicate = await _loanRepository.Query()
            .AnyAsync(l =>
                l.LoanApplicationId == dto.LoanApplicationId &&
                l.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "A loan has already been disbursed for this application.");
        }

        var disbursementDate = dto.DisbursementDate.Date;

        var firstStartOfMonth = new DateTime(dto.FirstInstallmentYear, dto.FirstInstallmentMonth, 1);
        var disbursementMonthStart = new DateTime(disbursementDate.Year, disbursementDate.Month, 1);

        if (firstStartOfMonth < disbursementMonthStart)
        {
            throw new InvalidOperationException(
                "FirstInstallmentMonth/Year must be on or after the disbursement month.");
        }

        var totalRepayable = Math.Round(
            approval.MonthlyInstallment.Value * approval.ApprovedTenureMonths.Value, 2);

        var loanNo = await GenerateLoanNoAsync(subscriptionId);
        var now = DateTime.UtcNow;

        var loan = new EmployeeLoan
        {
            LoanApplicationId = application.Id,
            EmployeeId = application.EmployeeId,
            LoanNo = loanNo,
            PrincipalAmount = approval.ApprovedAmount.Value,
            InterestRate = approval.InterestRate ?? 0m,
            InterestType = approval.InterestType,
            TenureMonths = approval.ApprovedTenureMonths.Value,
            MonthlyInstallment = approval.MonthlyInstallment.Value,
            TotalRepayable = totalRepayable,
            DisbursementDate = disbursementDate,
            FirstInstallmentMonth = dto.FirstInstallmentMonth,
            FirstInstallmentYear = dto.FirstInstallmentYear,
            TotalPaid = 0m,
            OutstandingBalance = totalRepayable,
            PaidInstallments = 0,
            Status = "Active",
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.EmployeeLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        var schedule = GenerateInstallmentSchedule(
            loan.Id, application.EmployeeId,
            loan.MonthlyInstallment, loan.TotalRepayable,
            loan.TenureMonths, loan.FirstInstallmentMonth, loan.FirstInstallmentYear,
            subscriptionId);

        foreach (var installment in schedule)
        {
            await _context.LoanInstallments.AddAsync(installment);
        }

        application.Status = "Disbursed";
        application.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(loan.Id, subscriptionId);
    }

    public async Task<EmployeeLoanResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<EmployeeLoanResponseDto> GetByEmployeeAsync(int employeeId)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var loan = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.EmployeeId == employeeId && l.Status == "Active")
            ?? throw new KeyNotFoundException("No active loan found for this employee.");

        return MapToResponseDto(loan);
    }

    public async Task<PagedResultDto<EmployeeLoanResponseDto>> GetFilteredAsync(EmployeeLoanFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(l => l.EmployeeId == empId);
        }

        if (filter.BranchId is int brId)
        {
            query = query.Where(l => l.Employee.BranchId == brId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(l => l.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.LoanType))
        {
            var loanType = filter.LoanType.Trim();
            query = query.Where(l => l.LoanApplication.LoanType == loanType);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<EmployeeLoanResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<EmployeeLoanResponseDto> MarkCompletedAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var loan = await _loanRepository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan.");
        }

        if (loan.Status != "Active")
        {
            throw new InvalidOperationException(
                $"Loan in status '{loan.Status}' cannot be marked complete.");
        }

        if (loan.PaidInstallments < loan.TenureMonths && loan.OutstandingBalance > 0)
        {
            throw new InvalidOperationException(
                "Loan cannot be marked complete — installments are still outstanding.");
        }

        var now = DateTime.UtcNow;
        loan.Status = "Completed";
        loan.OutstandingBalance = 0m;
        loan.UpdatedAt = now;

        await _loanRepository.UpdateAsync(loan);

        return await LoadResponseAsync(loan.Id, subscriptionId);
    }

    public async Task<EmployeeLoanResponseDto> MarkDefaultedAsync(int id, string reason)
    {
        var subscriptionId = GetSubscriptionId();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Reason is required when marking a loan as defaulted.");
        }

        var loan = await _loanRepository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan.");
        }

        if (loan.Status != "Active")
        {
            throw new InvalidOperationException(
                $"Loan in status '{loan.Status}' cannot be marked as defaulted.");
        }

        var now = DateTime.UtcNow;
        loan.Status = "Defaulted";
        loan.Remarks = string.IsNullOrWhiteSpace(loan.Remarks)
            ? reason
            : $"{loan.Remarks} | Defaulted: {reason}";
        loan.UpdatedAt = now;

        var pendingInstallments = await _installmentRepository.Query()
            .Where(i => i.EmployeeLoanId == loan.Id &&
                        (i.Status == "Pending" || i.Status == "Overdue"))
            .ToListAsync();

        foreach (var installment in pendingInstallments)
        {
            installment.Status = "Overdue";
            installment.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(loan.Id, subscriptionId);
    }

    public async Task<EmployeeLoanResponseDto> CancelAsync(int id, string reason)
    {
        var subscriptionId = GetSubscriptionId();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Reason is required when cancelling a loan.");
        }

        var loan = await _loanRepository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan.");
        }

        if (loan.Status != "Active")
        {
            throw new InvalidOperationException(
                $"Loan in status '{loan.Status}' cannot be cancelled.");
        }

        if (loan.PaidInstallments > 0)
        {
            throw new InvalidOperationException(
                "Cannot cancel a loan with payments already made. Mark as Defaulted instead.");
        }

        var now = DateTime.UtcNow;
        loan.Status = "Cancelled";
        loan.Remarks = string.IsNullOrWhiteSpace(loan.Remarks)
            ? reason
            : $"{loan.Remarks} | Cancelled: {reason}";
        loan.UpdatedAt = now;

        var pendingInstallments = await _installmentRepository.Query()
            .Where(i => i.EmployeeLoanId == loan.Id && i.Status == "Pending")
            .ToListAsync();

        foreach (var installment in pendingInstallments)
        {
            installment.Status = "Skipped";
            installment.UpdatedAt = now;
        }

        var application = await _applicationRepository.Query()
            .FirstOrDefaultAsync(la => la.Id == loan.LoanApplicationId);

        if (application is not null)
        {
            application.Status = "Cancelled";
            application.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        return await LoadResponseAsync(loan.Id, subscriptionId);
    }

    private IQueryable<EmployeeLoan> BaseQuery(int subscriptionId)
    {
        return _loanRepository
            .Query()
            .Include(l => l.Employee)
            .Include(l => l.LoanApplication)
            .Include(l => l.Installments)
            .Where(l => l.SubscriptionId == subscriptionId);
    }

    private async Task<EmployeeLoanResponseDto> LoadResponseAsync(int loanId, int subscriptionId)
    {
        var loan = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan is null)
        {
            var existsForOtherTenant = await _loanRepository.Query()
                .AnyAsync(l => l.Id == loanId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this loan.");
            }

            throw new KeyNotFoundException($"Loan with ID {loanId} not found.");
        }

        return MapToResponseDto(loan);
    }

    private EmployeeLoanResponseDto MapToResponseDto(EmployeeLoan l)
    {
        var dto = _mapper.Map<EmployeeLoanResponseDto>(l);

        dto.ApplicationNo = l.LoanApplication?.ApplicationNo ?? string.Empty;
        dto.LoanType = l.LoanApplication?.LoanType ?? string.Empty;
        dto.EmployeeCode = l.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = l.Employee?.FullName ?? string.Empty;

        dto.PrincipalAmountFormatted = l.PrincipalAmount.ToString("N2");
        dto.MonthlyInstallmentFormatted = l.MonthlyInstallment.ToString("N2");
        dto.TotalRepayableFormatted = l.TotalRepayable.ToString("N2");
        dto.TotalPaidFormatted = l.TotalPaid.ToString("N2");
        dto.OutstandingBalanceFormatted = l.OutstandingBalance.ToString("N2");
        dto.DisbursementDateFormatted = l.DisbursementDate.ToString("dd MMM yyyy");

        dto.TenureLabel = FormatTenure(l.TenureMonths);

        dto.InterestTypeLabel = l.InterestRate == 0m
            ? "Interest-Free"
            : l.InterestType == "Flat" ? "Flat Rate" : "Reducing Balance";

        dto.FirstInstallmentPeriodLabel =
            new DateTime(l.FirstInstallmentYear, l.FirstInstallmentMonth, 1).ToString("MMMM yyyy");

        dto.RemainingInstallments = l.TenureMonths - l.PaidInstallments;
        dto.CompletionPercentage = l.TenureMonths > 0
            ? Math.Round((decimal)l.PaidInstallments / l.TenureMonths * 100m, 2)
            : 0m;

        dto.StatusLabel = l.Status switch
        {
            "Active" => "Active",
            "Completed" => "Completed",
            "Defaulted" => "Defaulted",
            "Cancelled" => "Cancelled",
            _ => l.Status
        };

        dto.Installments = l.Installments
            .OrderBy(i => i.InstallmentNo)
            .Select(MapInstallmentDto)
            .ToList();

        return dto;
    }

    private static LoanInstallmentSummaryDto MapInstallmentDto(LoanInstallment i)
    {
        return new LoanInstallmentSummaryDto
        {
            Id = i.Id,
            InstallmentNo = i.InstallmentNo,
            DueMonth = i.DueMonth,
            DueYear = i.DueYear,
            DuePeriodLabel = new DateTime(i.DueYear, i.DueMonth, 1).ToString("MMMM yyyy"),
            InstallmentAmount = i.InstallmentAmount,
            InstallmentAmountFormatted = i.InstallmentAmount.ToString("N2"),
            PaidAmount = i.PaidAmount,
            Status = i.Status,
            StatusLabel = i.Status switch
            {
                "Pending" => "Pending",
                "Paid" => "Paid",
                "Skipped" => "Skipped",
                "Overdue" => "Overdue",
                _ => i.Status
            },
            PaidDate = i.PaidDate
        };
    }

    private static List<LoanInstallment> GenerateInstallmentSchedule(
        int employeeLoanId, int employeeId,
        decimal monthlyInstallment, decimal totalRepayable,
        int tenureMonths, int firstMonth, int firstYear,
        int subscriptionId)
    {
        var installments = new List<LoanInstallment>(tenureMonths);
        decimal accumulated = 0m;
        int month = firstMonth;
        int year = firstYear;
        var now = DateTime.UtcNow;

        for (int i = 1; i <= tenureMonths; i++)
        {
            decimal amount = (i == tenureMonths)
                ? Math.Round(totalRepayable - accumulated, 2)
                : monthlyInstallment;

            installments.Add(new LoanInstallment
            {
                EmployeeLoanId = employeeLoanId,
                EmployeeId = employeeId,
                InstallmentNo = i,
                DueMonth = month,
                DueYear = year,
                InstallmentAmount = amount,
                PaidAmount = 0m,
                Status = "Pending",
                SubscriptionId = subscriptionId,
                CreatedAt = now,
                UpdatedAt = now
            });

            accumulated += amount;

            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }
        }

        return installments;
    }

    private async Task<string> GenerateLoanNoAsync(int subscriptionId)
    {
        int year = DateTime.UtcNow.Year;
        int count = await _loanRepository.Query()
            .CountAsync(l =>
                l.CreatedAt.Year == year &&
                l.SubscriptionId == subscriptionId);
        return $"LOAN-{year}-{(count + 1):D4}";
    }

    private static string FormatTenure(int months)
    {
        int y = months / 12, r = months % 12;
        if (y > 0 && r > 0) return $"{months} months ({y} year(s) {r} month(s))";
        if (y > 0) return $"{months} months ({y} year(s))";
        return $"{months} month(s)";
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

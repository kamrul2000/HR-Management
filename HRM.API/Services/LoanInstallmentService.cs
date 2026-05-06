using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LoanInstallment;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LoanInstallmentService : ILoanInstallmentService
{
    private const int MaxPageSize = 200;

    private readonly IRepository<LoanInstallment> _installmentRepository;
    private readonly IRepository<EmployeeLoan> _loanRepository;
    private readonly IEmployeeLoanService _employeeLoanService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public LoanInstallmentService(
        IRepository<LoanInstallment> installmentRepository,
        IRepository<EmployeeLoan> loanRepository,
        IEmployeeLoanService employeeLoanService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _installmentRepository = installmentRepository;
        _loanRepository = loanRepository;
        _employeeLoanService = employeeLoanService;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<LoanInstallmentResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<LoanInstallmentResponseDto>> GetFilteredAsync(InstallmentFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(i => i.EmployeeId == empId);
        }

        if (filter.EmployeeLoanId is int loanId)
        {
            query = query.Where(i => i.EmployeeLoanId == loanId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(i => i.Status == status);
        }

        if (filter.DueMonth is int month)
        {
            query = query.Where(i => i.DueMonth == month);
        }

        if (filter.DueYear is int year)
        {
            query = query.Where(i => i.DueYear == year);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.DueYear)
            .ThenByDescending(i => i.DueMonth)
            .ThenBy(i => i.InstallmentNo)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<LoanInstallmentResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<LoanInstallmentResponseDto>> GetByLoanAsync(int employeeLoanId)
    {
        var subscriptionId = GetSubscriptionId();

        var loan = await _loanRepository.GetByIdAsync(employeeLoanId)
            ?? throw new KeyNotFoundException($"Employee loan with ID {employeeLoanId} not found.");

        if (loan.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan.");
        }

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(i => i.EmployeeLoanId == employeeLoanId)
            .OrderBy(i => i.InstallmentNo)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<PendingInstallmentDto?> GetPendingInstallmentAsync(int employeeId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();

        var pending = await _installmentRepository.Query()
            .AsNoTracking()
            .Where(i =>
                i.EmployeeId == employeeId &&
                i.SubscriptionId == subscriptionId &&
                i.DueYear == year &&
                i.DueMonth == month &&
                (i.Status == "Pending" || i.Status == "Overdue"))
            .OrderBy(i => i.InstallmentNo)
            .FirstOrDefaultAsync();

        if (pending is null)
        {
            return null;
        }

        return new PendingInstallmentDto
        {
            InstallmentId = pending.Id,
            EmployeeLoanId = pending.EmployeeLoanId,
            InstallmentAmount = pending.InstallmentAmount,
            InstallmentNo = pending.InstallmentNo,
            DueMonth = pending.DueMonth,
            DueYear = pending.DueYear
        };
    }

    public async Task<LoanInstallmentResponseDto> ProcessPaymentAsync(int id, ProcessInstallmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var installment = await _installmentRepository.Query()
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Loan installment with ID {id} not found.");

        if (installment.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this installment.");
        }

        if (installment.Status != "Pending" && installment.Status != "Overdue")
        {
            throw new InvalidOperationException(
                $"Cannot process payment on a '{installment.Status}' installment.");
        }

        var paidAmount = dto.PaidAmount ?? installment.InstallmentAmount;
        if (paidAmount <= 0)
        {
            throw new InvalidOperationException("PaidAmount must be greater than zero.");
        }

        var loan = await _loanRepository.GetByIdAsync(installment.EmployeeLoanId)
            ?? throw new KeyNotFoundException($"Parent loan with ID {installment.EmployeeLoanId} not found.");

        var now = DateTime.UtcNow;
        installment.Status = "Paid";
        installment.PaidAmount = paidAmount;
        installment.PaidDate = now;
        installment.SalaryCalculationId = dto.SalaryCalculationId;
        installment.Remarks = dto.Remarks ?? installment.Remarks;
        installment.UpdatedAt = now;

        loan.TotalPaid += paidAmount;
        loan.PaidInstallments += 1;
        loan.OutstandingBalance = Math.Max(0m, loan.TotalRepayable - loan.TotalPaid);
        loan.UpdatedAt = now;

        await _context.SaveChangesAsync();

        if (loan.PaidInstallments >= loan.TenureMonths &&
            loan.OutstandingBalance <= 0m &&
            loan.Status == "Active")
        {
            await _employeeLoanService.MarkCompletedAsync(loan.Id);
        }

        return await LoadResponseAsync(installment.Id, subscriptionId);
    }

    public async Task<LoanInstallmentResponseDto> SkipAsync(int id, SkipInstallmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var installment = await _installmentRepository.Query()
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Loan installment with ID {id} not found.");

        if (installment.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this installment.");
        }

        if (installment.Status != "Pending" && installment.Status != "Overdue")
        {
            throw new InvalidOperationException(
                $"Cannot skip a '{installment.Status}' installment.");
        }

        installment.Status = "Skipped";
        installment.Remarks = dto.Reason;
        installment.UpdatedAt = DateTime.UtcNow;

        await _installmentRepository.UpdateAsync(installment);

        return await LoadResponseAsync(installment.Id, subscriptionId);
    }

    public async Task<LoanInstallmentResponseDto> ReinstateAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var installment = await _installmentRepository.Query()
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Loan installment with ID {id} not found.");

        if (installment.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this installment.");
        }

        if (installment.Status != "Skipped")
        {
            throw new InvalidOperationException("Only skipped installments can be reinstated.");
        }

        var loan = await _loanRepository.GetByIdAsync(installment.EmployeeLoanId)
            ?? throw new KeyNotFoundException($"Parent loan with ID {installment.EmployeeLoanId} not found.");

        if (loan.Status != "Active")
        {
            throw new InvalidOperationException(
                $"Cannot reinstate installment — parent loan is in status '{loan.Status}'.");
        }

        installment.Status = "Pending";
        installment.Remarks = null;
        installment.UpdatedAt = DateTime.UtcNow;

        await _installmentRepository.UpdateAsync(installment);

        return await LoadResponseAsync(installment.Id, subscriptionId);
    }

    public async Task<int> MarkOverdueAsync(int year, int month)
    {
        // Cross-tenant batch operation — no subscription filter.
        var pending = await _installmentRepository.Query()
            .Where(i =>
                i.Status == "Pending" &&
                (i.DueYear < year || (i.DueYear == year && i.DueMonth < month)))
            .ToListAsync();

        if (pending.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var installment in pending)
        {
            installment.Status = "Overdue";
            installment.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return pending.Count;
    }

    private IQueryable<LoanInstallment> BaseQuery(int subscriptionId)
    {
        return _installmentRepository
            .Query()
            .Include(i => i.EmployeeLoan)
            .Include(i => i.Employee)
            .Where(i => i.SubscriptionId == subscriptionId);
    }

    private async Task<LoanInstallmentResponseDto> LoadResponseAsync(int installmentId, int subscriptionId)
    {
        var installment = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment is null)
        {
            var existsForOtherTenant = await _installmentRepository.Query()
                .AnyAsync(i => i.Id == installmentId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this installment.");
            }

            throw new KeyNotFoundException($"Loan installment with ID {installmentId} not found.");
        }

        return MapToResponseDto(installment);
    }

    private LoanInstallmentResponseDto MapToResponseDto(LoanInstallment i)
    {
        var dto = _mapper.Map<LoanInstallmentResponseDto>(i);

        dto.LoanNo = i.EmployeeLoan?.LoanNo ?? string.Empty;
        dto.EmployeeCode = i.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = i.Employee?.FullName ?? string.Empty;

        dto.DuePeriodLabel = new DateTime(i.DueYear, i.DueMonth, 1).ToString("MMMM yyyy");
        dto.InstallmentAmountFormatted = i.InstallmentAmount.ToString("N2");
        dto.PaidAmountFormatted = i.PaidAmount.ToString("N2");
        dto.PaidDateFormatted = i.PaidDate?.ToString("dd MMM yyyy");

        dto.StatusLabel = i.Status switch
        {
            "Pending" => "Pending",
            "Paid" => "Paid",
            "Skipped" => "Skipped",
            "Overdue" => "Overdue",
            _ => i.Status
        };

        return dto;
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

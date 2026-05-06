using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LoanApproval;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LoanApprovalService : ILoanApprovalService
{
    private readonly IRepository<LoanApproval> _approvalRepository;
    private readonly IRepository<LoanApplication> _loanApplicationRepository;
    private readonly IRepository<LoanRecommendation> _recommendationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public LoanApprovalService(
        IRepository<LoanApproval> approvalRepository,
        IRepository<LoanApplication> loanApplicationRepository,
        IRepository<LoanRecommendation> recommendationRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _approvalRepository = approvalRepository;
        _loanApplicationRepository = loanApplicationRepository;
        _recommendationRepository = recommendationRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<LoanApprovalResponseDto> ProcessAsync(CreateLoanApprovalDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        ValidateDecision(dto.Decision);
        ValidateApprovalFields(dto);

        var loanApplication = await _loanApplicationRepository.Query()
            .Include(la => la.Employee)
            .FirstOrDefaultAsync(la => la.Id == dto.LoanApplicationId)
            ?? throw new KeyNotFoundException($"Loan application with ID {dto.LoanApplicationId} not found.");

        if (loanApplication.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        if (loanApplication.Status != "Recommended")
        {
            throw new InvalidOperationException(
                $"Cannot process a loan application with status '{loanApplication.Status}'. " +
                "Only 'Recommended' applications can be approved or rejected.");
        }

        var duplicate = await _approvalRepository.Query()
            .AnyAsync(a =>
                a.LoanApplicationId == dto.LoanApplicationId &&
                a.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "An approval decision has already been recorded for this loan application.");
        }

        decimal? monthlyInstallment = null;

        if (dto.Decision == "Approved")
        {
            if (dto.ApprovedAmount!.Value > loanApplication.RequestedAmount)
            {
                throw new InvalidOperationException(
                    $"Approved amount ({dto.ApprovedAmount.Value:N2}) cannot exceed the requested amount ({loanApplication.RequestedAmount:N2}).");
            }

            monthlyInstallment = ComputeMonthlyInstallment(
                dto.ApprovedAmount.Value,
                dto.ApprovedTenureMonths!.Value,
                dto.InterestRate,
                dto.InterestType);
        }

        var now = DateTime.UtcNow;

        var approval = new LoanApproval
        {
            LoanApplicationId = loanApplication.Id,
            ApprovedById = callerId,
            Decision = dto.Decision,
            ApprovedAmount = dto.Decision == "Approved" ? dto.ApprovedAmount : null,
            ApprovedTenureMonths = dto.Decision == "Approved" ? dto.ApprovedTenureMonths : null,
            MonthlyInstallment = monthlyInstallment,
            InterestRate = dto.Decision == "Approved" ? dto.InterestRate : null,
            InterestType = dto.Decision == "Approved" ? dto.InterestType : null,
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        loanApplication.Status = dto.Decision;
        loanApplication.UpdatedAt = now;

        if (dto.Decision == "Rejected")
        {
            loanApplication.RejectedById = callerId;
            loanApplication.RejectionDate = now;
            loanApplication.RejectionRemarks = dto.Remarks;
        }

        await _context.LoanApprovals.AddAsync(approval);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(approval.Id, subscriptionId);
    }

    public async Task<LoanApprovalResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<LoanApprovalResponseDto> GetByLoanApplicationAsync(int loanApplicationId)
    {
        var subscriptionId = GetSubscriptionId();

        var loanApplication = await _loanApplicationRepository.GetByIdAsync(loanApplicationId)
            ?? throw new KeyNotFoundException($"Loan application with ID {loanApplicationId} not found.");

        if (loanApplication.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        var approval = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.LoanApplicationId == loanApplicationId)
            ?? throw new KeyNotFoundException("No approval record found for this loan application.");

        return await MapToResponseDtoAsync(approval, subscriptionId);
    }

    public async Task<IEnumerable<LoanApprovalResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var results = new List<LoanApprovalResponseDto>(items.Count);
        foreach (var item in items)
        {
            results.Add(await MapToResponseDtoAsync(item, subscriptionId));
        }
        return results;
    }

    private IQueryable<LoanApproval> BaseQuery(int subscriptionId)
    {
        return _approvalRepository
            .Query()
            .Include(a => a.LoanApplication)
                .ThenInclude(la => la.Employee)
            .Where(a => a.SubscriptionId == subscriptionId);
    }

    private async Task<LoanApprovalResponseDto> LoadResponseAsync(int approvalId, int subscriptionId)
    {
        var approval = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == approvalId);

        if (approval is null)
        {
            var existsForOtherTenant = await _approvalRepository.Query()
                .AnyAsync(a => a.Id == approvalId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this loan approval.");
            }

            throw new KeyNotFoundException($"Loan approval with ID {approvalId} not found.");
        }

        return await MapToResponseDtoAsync(approval, subscriptionId);
    }

    private async Task<LoanApprovalResponseDto> MapToResponseDtoAsync(LoanApproval a, int subscriptionId)
    {
        var dto = _mapper.Map<LoanApprovalResponseDto>(a);

        dto.ApplicationNo = a.LoanApplication?.ApplicationNo ?? string.Empty;
        dto.EmployeeId = a.LoanApplication?.EmployeeId ?? 0;
        dto.EmployeeCode = a.LoanApplication?.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = a.LoanApplication?.Employee?.FullName ?? string.Empty;
        dto.RequestedAmount = a.LoanApplication?.RequestedAmount ?? 0m;

        var recommendation = await _recommendationRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.LoanApplicationId == a.LoanApplicationId &&
                r.SubscriptionId == subscriptionId);
        dto.RecommendedAmount = recommendation?.RecommendedAmount;

        dto.DecisionLabel = a.Decision switch
        {
            "Approved" => "Approved by HR",
            "Rejected" => "Rejected by HR",
            _ => a.Decision
        };

        dto.ApprovedAmountFormatted = a.ApprovedAmount?.ToString("N2");

        if (a.ApprovedTenureMonths.HasValue)
        {
            dto.TenureLabel = FormatTenure(a.ApprovedTenureMonths.Value);
        }

        if (a.Decision == "Approved")
        {
            dto.InterestTypeLabel = (a.InterestRate is null or 0)
                ? "Interest-Free"
                : a.InterestType == "Flat" ? "Flat Rate" : "Reducing Balance";
        }

        dto.MonthlyInstallmentFormatted = a.MonthlyInstallment?.ToString("N2");

        if (a.MonthlyInstallment.HasValue && a.ApprovedTenureMonths.HasValue)
        {
            dto.TotalRepayable = Math.Round(a.MonthlyInstallment.Value * a.ApprovedTenureMonths.Value, 2);
            dto.TotalRepayableFormatted = dto.TotalRepayable?.ToString("N2");
        }

        return dto;
    }

    private static decimal ComputeMonthlyInstallment(
        decimal amount, int tenureMonths, decimal? interestRate, string? interestType)
    {
        if (!interestRate.HasValue || interestRate.Value == 0m)
        {
            return Math.Round(amount / tenureMonths, 2);
        }

        decimal annualRate = interestRate.Value / 100m;

        if (interestType == "Flat")
        {
            decimal totalInterest = amount * annualRate * (tenureMonths / 12m);
            decimal totalRepayable = amount + totalInterest;
            return Math.Round(totalRepayable / tenureMonths, 2);
        }

        // Reducing balance EMI
        decimal monthlyRate = annualRate / 12m;
        if (monthlyRate == 0m)
        {
            return Math.Round(amount / tenureMonths, 2);
        }

        double r = (double)monthlyRate;
        int n = tenureMonths;
        double emi = (double)amount * (r * Math.Pow(1 + r, n)) / (Math.Pow(1 + r, n) - 1);
        return Math.Round((decimal)emi, 2);
    }

    private static string FormatTenure(int months)
    {
        int years = months / 12;
        int rem = months % 12;
        if (years > 0 && rem > 0) return $"{months} months ({years} year(s) {rem} month(s))";
        if (years > 0) return $"{months} months ({years} year(s))";
        return $"{months} month(s)";
    }

    private static void ValidateDecision(string decision)
    {
        if (decision != "Approved" && decision != "Rejected")
        {
            throw new InvalidOperationException("Decision must be 'Approved' or 'Rejected'.");
        }
    }

    private static void ValidateApprovalFields(CreateLoanApprovalDto dto)
    {
        if (dto.Decision == "Approved")
        {
            if (!dto.ApprovedAmount.HasValue || dto.ApprovedAmount.Value <= 0)
            {
                throw new InvalidOperationException(
                    "ApprovedAmount is required and must be > 0 when Decision is 'Approved'.");
            }

            if (!dto.ApprovedTenureMonths.HasValue || dto.ApprovedTenureMonths.Value < 1)
            {
                throw new InvalidOperationException(
                    "ApprovedTenureMonths is required and must be >= 1 when Decision is 'Approved'.");
            }

            if (dto.InterestRate.HasValue && dto.InterestRate.Value > 0)
            {
                if (string.IsNullOrWhiteSpace(dto.InterestType))
                {
                    throw new InvalidOperationException(
                        "InterestType ('Flat' or 'Reducing') is required when InterestRate > 0.");
                }

                if (dto.InterestType != "Flat" && dto.InterestType != "Reducing")
                {
                    throw new InvalidOperationException(
                        "InterestType must be 'Flat' or 'Reducing'.");
                }
            }
        }
        else
        {
            if (dto.ApprovedAmount.HasValue || dto.ApprovedTenureMonths.HasValue)
            {
                throw new InvalidOperationException(
                    "ApprovedAmount and ApprovedTenureMonths must not be provided when Decision is 'Rejected'.");
            }
        }
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

using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LoanRecommendation;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LoanRecommendationService : ILoanRecommendationService
{
    private readonly IRepository<LoanRecommendation> _recommendationRepository;
    private readonly IRepository<LoanApplication> _loanApplicationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public LoanRecommendationService(
        IRepository<LoanRecommendation> recommendationRepository,
        IRepository<LoanApplication> loanApplicationRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _recommendationRepository = recommendationRepository;
        _loanApplicationRepository = loanApplicationRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<LoanRecommendationResponseDto> RecommendAsync(CreateRecommendationDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        ValidateDecision(dto.Decision);

        var loanApplication = await _loanApplicationRepository.Query()
            .Include(la => la.Employee)
            .FirstOrDefaultAsync(la => la.Id == dto.LoanApplicationId)
            ?? throw new KeyNotFoundException($"Loan application with ID {dto.LoanApplicationId} not found.");

        if (loanApplication.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        if (loanApplication.Status != "Pending")
        {
            throw new InvalidOperationException(
                $"Cannot recommend a loan application with status '{loanApplication.Status}'. " +
                "Only 'Pending' applications can be reviewed.");
        }

        var duplicate = await _recommendationRepository.Query()
            .AnyAsync(r =>
                r.LoanApplicationId == dto.LoanApplicationId &&
                r.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "A recommendation has already been submitted for this loan application.");
        }

        if (dto.Decision == "Recommended" &&
            dto.RecommendedAmount.HasValue &&
            dto.RecommendedAmount.Value > loanApplication.RequestedAmount)
        {
            throw new InvalidOperationException(
                $"Recommended amount ({dto.RecommendedAmount.Value:N2}) cannot exceed the requested amount ({loanApplication.RequestedAmount:N2}).");
        }

        var now = DateTime.UtcNow;

        var recommendation = new LoanRecommendation
        {
            LoanApplicationId = loanApplication.Id,
            RecommendedById = callerId,
            Decision = dto.Decision,
            RecommendedAmount = dto.Decision == "Recommended" ? dto.RecommendedAmount : null,
            RecommendedTenureMonths = dto.Decision == "Recommended" ? dto.RecommendedTenureMonths : null,
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        loanApplication.Status = dto.Decision;
        loanApplication.UpdatedAt = now;

        if (dto.Decision == "Recommended")
        {
            loanApplication.RecommendedById = callerId;
            loanApplication.RecommendationDate = now;
            loanApplication.RecommendationRemarks = dto.Remarks;
        }
        else
        {
            loanApplication.RejectedById = callerId;
            loanApplication.RejectionDate = now;
            loanApplication.RejectionRemarks = dto.Remarks;
        }

        await _context.LoanRecommendations.AddAsync(recommendation);
        await _context.SaveChangesAsync();

        return await LoadResponseAsync(recommendation.Id, subscriptionId);
    }

    public async Task<LoanRecommendationResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<LoanRecommendationResponseDto> GetByLoanApplicationAsync(int loanApplicationId)
    {
        var subscriptionId = GetSubscriptionId();

        var loanApplication = await _loanApplicationRepository.GetByIdAsync(loanApplicationId)
            ?? throw new KeyNotFoundException($"Loan application with ID {loanApplicationId} not found.");

        if (loanApplication.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this loan application.");
        }

        var recommendation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.LoanApplicationId == loanApplicationId)
            ?? throw new KeyNotFoundException("No recommendation found for this loan application.");

        return MapToResponseDto(recommendation);
    }

    public async Task<IEnumerable<LoanRecommendationResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    private IQueryable<LoanRecommendation> BaseQuery(int subscriptionId)
    {
        return _recommendationRepository
            .Query()
            .Include(r => r.LoanApplication)
                .ThenInclude(la => la.Employee)
            .Where(r => r.SubscriptionId == subscriptionId);
    }

    private async Task<LoanRecommendationResponseDto> LoadResponseAsync(int recommendationId, int subscriptionId)
    {
        var recommendation = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recommendationId);

        if (recommendation is null)
        {
            var existsForOtherTenant = await _recommendationRepository.Query()
                .AnyAsync(r => r.Id == recommendationId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this recommendation.");
            }

            throw new KeyNotFoundException($"Loan recommendation with ID {recommendationId} not found.");
        }

        return MapToResponseDto(recommendation);
    }

    private LoanRecommendationResponseDto MapToResponseDto(LoanRecommendation r)
    {
        var dto = _mapper.Map<LoanRecommendationResponseDto>(r);

        dto.ApplicationNo = r.LoanApplication?.ApplicationNo ?? string.Empty;
        dto.EmployeeId = r.LoanApplication?.EmployeeId ?? 0;
        dto.EmployeeCode = r.LoanApplication?.Employee?.EmployeeCode ?? string.Empty;
        dto.EmployeeFullName = r.LoanApplication?.Employee?.FullName ?? string.Empty;
        dto.RequestedAmount = r.LoanApplication?.RequestedAmount ?? 0m;
        dto.RequestedAmountFormatted = (r.LoanApplication?.RequestedAmount ?? 0m).ToString("N2");
        dto.RequestedTenureMonths = r.LoanApplication?.RequestedTenureMonths ?? 0;

        dto.DecisionLabel = r.Decision switch
        {
            "Recommended" => "Recommended for Approval",
            "Rejected" => "Rejected by Supervisor",
            _ => r.Decision
        };

        dto.RecommendedAmountFormatted = r.RecommendedAmount.HasValue
            ? r.RecommendedAmount.Value.ToString("N2")
            : null;

        dto.AmountDifference = r.RecommendedAmount.HasValue && r.LoanApplication is not null
            ? r.LoanApplication.RequestedAmount - r.RecommendedAmount.Value
            : null;

        return dto;
    }

    private static void ValidateDecision(string decision)
    {
        if (decision != "Recommended" && decision != "Rejected")
        {
            throw new InvalidOperationException(
                "Decision must be 'Recommended' or 'Rejected'.");
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

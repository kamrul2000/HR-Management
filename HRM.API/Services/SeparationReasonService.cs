using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.SeparationReason;
using HRM.Core.Entities;
using HRM.Infrastructure.Data;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class SeparationReasonService : ISeparationReasonService
{
    private static readonly string[] ValidSeparationTypes =
        { "Resignation", "Termination", "Retirement", "Redundancy", "Death", "All" };

    private readonly IRepository<SeparationReason> _reasonRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public SeparationReasonService(
        IRepository<SeparationReason> reasonRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        AppDbContext context)
    {
        _reasonRepository = reasonRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _context = context;
    }

    public async Task<SeparationReasonResponseDto> CreateAsync(CreateSeparationReasonDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateSeparationType(dto.SeparationType);

        var trimmedName = dto.ReasonName.Trim();
        await EnsureUniqueAsync(trimmedName, dto.SeparationType, subscriptionId);

        var now = DateTime.UtcNow;
        var reason = new SeparationReason
        {
            ReasonName = trimmedName,
            SeparationType = dto.SeparationType,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.SeparationReasons.AddAsync(reason);
        await _context.SaveChangesAsync();

        return MapToResponseDto(reason);
    }

    public async Task<SeparationReasonResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        var reason = await LoadReasonAsync(id, subscriptionId);
        return MapToResponseDto(reason);
    }

    public async Task<IEnumerable<SeparationReasonResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var reasons = await _reasonRepository.Query()
            .AsNoTracking()
            .Where(r => r.SubscriptionId == subscriptionId)
            .OrderBy(r => r.SeparationType)
            .ThenBy(r => r.DisplayOrder)
            .ToListAsync();

        return reasons.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<SeparationReasonResponseDto>> GetBySeparationTypeAsync(string separationType)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateSeparationType(separationType);

        var reasons = await _reasonRepository.Query()
            .AsNoTracking()
            .Where(r =>
                r.SubscriptionId == subscriptionId &&
                r.IsActive &&
                (r.SeparationType == separationType || r.SeparationType == "All"))
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync();

        return reasons.Select(MapToResponseDto).ToList();
    }

    public async Task<SeparationReasonResponseDto> UpdateAsync(int id, UpdateSeparationReasonDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        ValidateSeparationType(dto.SeparationType);

        var reason = await LoadReasonAsync(id, subscriptionId);
        var trimmedName = dto.ReasonName.Trim();

        var nameOrTypeChanged =
            !string.Equals(reason.ReasonName, trimmedName, StringComparison.Ordinal) ||
            !string.Equals(reason.SeparationType, dto.SeparationType, StringComparison.Ordinal);

        if (nameOrTypeChanged)
        {
            await EnsureUniqueAsync(trimmedName, dto.SeparationType, subscriptionId, id);
        }

        reason.ReasonName = trimmedName;
        reason.SeparationType = dto.SeparationType;
        reason.Description = dto.Description;
        reason.DisplayOrder = dto.DisplayOrder;
        reason.IsActive = dto.IsActive;
        reason.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponseDto(reason);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        var reason = await LoadReasonAsync(id, subscriptionId);

        var inUse = await _context.EmployeeSeparations
            .AnyAsync(s => s.SeparationReasonId == id);

        if (inUse)
        {
            throw new InvalidOperationException(
                "Cannot delete a separation reason that has been used in employee separations.");
        }

        await _reasonRepository.DeleteAsync(reason);
    }

    private async Task<SeparationReason> LoadReasonAsync(int id, int subscriptionId)
    {
        var reason = await _reasonRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reason is null)
        {
            throw new KeyNotFoundException($"Separation reason with ID {id} not found.");
        }

        if (reason.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this separation reason.");
        }

        return reason;
    }

    private async Task EnsureUniqueAsync(
        string reasonName, string separationType,
        int subscriptionId, int? excludeId = null)
    {
        var exists = await _reasonRepository.Query()
            .AnyAsync(r =>
                r.ReasonName == reasonName &&
                r.SeparationType == separationType &&
                r.SubscriptionId == subscriptionId &&
                (excludeId == null || r.Id != excludeId));

        if (exists)
        {
            throw new InvalidOperationException(
                $"A reason named '{reasonName}' already exists for SeparationType '{separationType}'.");
        }
    }

    private static void ValidateSeparationType(string separationType)
    {
        if (!ValidSeparationTypes.Contains(separationType))
        {
            throw new InvalidOperationException(
                $"Invalid SeparationType '{separationType}'. " +
                $"Accepted: {string.Join(", ", ValidSeparationTypes)}.");
        }
    }

    private SeparationReasonResponseDto MapToResponseDto(SeparationReason r)
    {
        var dto = _mapper.Map<SeparationReasonResponseDto>(r);

        dto.SeparationTypeLabel = r.SeparationType switch
        {
            "Resignation" => "Voluntary Resignation",
            "Termination" => "Termination by Employer",
            "Retirement" => "Retirement",
            "Redundancy" => "Redundancy / Lay-off",
            "Death" => "Death in Service",
            "All" => "All Types",
            _ => r.SeparationType
        };

        return dto;
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

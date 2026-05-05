using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.OffDay;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class OffDayService : IOffDayService
{
    private readonly IRepository<OffDay> _offDayRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public OffDayService(
        IRepository<OffDay> offDayRepository,
        IRepository<Branch> branchRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _offDayRepository = offDayRepository;
        _branchRepository = branchRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<OffDayResponseDto> CreateAsync(CreateOffDayDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        if (dto.BranchId.HasValue)
        {
            await ValidateBranchAsync(dto.BranchId.Value, subscriptionId);
        }

        await EnsureDayUniqueAsync(dto.DayOfWeek, dto.BranchId, subscriptionId);

        var now = DateTime.UtcNow;
        var offDay = _mapper.Map<OffDay>(dto);
        offDay.DayName = GetDayName(dto.DayOfWeek);
        offDay.SubscriptionId = subscriptionId;
        offDay.IsActive = true;
        offDay.CreatedAt = now;
        offDay.UpdatedAt = now;

        await _offDayRepository.AddAsync(offDay);

        return await LoadResponseAsync(offDay.Id, subscriptionId);
    }

    public async Task<IEnumerable<OffDayResponseDto>> BulkSetAsync(BulkSetOffDaysDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        if (dto.BranchId.HasValue)
        {
            await ValidateBranchAsync(dto.BranchId.Value, subscriptionId);
        }

        if (dto.DaysOfWeek.Count > 6)
        {
            throw new InvalidOperationException(
                "Cannot set all 7 days as off days — at least one working day is required.");
        }

        if (dto.DaysOfWeek.Count > 0)
        {
            ValidateDaysOfWeek(dto.DaysOfWeek);
        }

        var existing = await _offDayRepository.Query()
            .Where(o => o.SubscriptionId == subscriptionId && o.BranchId == dto.BranchId)
            .ToListAsync();

        foreach (var record in existing)
        {
            await _offDayRepository.DeleteAsync(record);
        }

        var now = DateTime.UtcNow;
        var created = new List<OffDay>();

        foreach (var dayOfWeek in dto.DaysOfWeek)
        {
            var offDay = new OffDay
            {
                DayOfWeek = dayOfWeek,
                DayName = GetDayName(dayOfWeek),
                BranchId = dto.BranchId,
                SubscriptionId = subscriptionId,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _offDayRepository.AddAsync(offDay);
            created.Add(offDay);
        }

        if (created.Count == 0)
        {
            return Array.Empty<OffDayResponseDto>();
        }

        var ids = created.Select(c => c.Id).ToList();

        var refreshed = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .OrderBy(o => o.DayOfWeek)
            .ToListAsync();

        return refreshed.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<OffDayResponseDto>> GetAllAsync(int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (branchId is int filterBranchId)
        {
            if (filterBranchId == 0)
            {
                query = query.Where(o => o.BranchId == null);
            }
            else
            {
                query = query.Where(o => o.BranchId == filterBranchId);
            }
        }

        var items = await query
            .OrderBy(o => o.BranchId.HasValue ? 1 : 0)
            .ThenBy(o => o.BranchId)
            .ThenBy(o => o.DayOfWeek)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<OffDayResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<OffDayScheduleDto> GetResolvedScheduleAsync(int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();

        var configurationSource = "Organization";
        int? resolvedBranchId = null;
        string? resolvedBranchName = null;

        List<OffDay>? records = null;

        if (branchId.HasValue)
        {
            await ValidateBranchAsync(branchId.Value, subscriptionId);

            records = await BaseQuery(subscriptionId)
                .AsNoTracking()
                .Where(o => o.BranchId == branchId && o.IsActive)
                .ToListAsync();

            if (records.Count > 0)
            {
                configurationSource = "Branch";
                resolvedBranchId = branchId;
                resolvedBranchName = records.First().Branch?.Name;
            }
            else
            {
                records = null; // trigger fallback below
            }
        }

        if (records is null)
        {
            records = await BaseQuery(subscriptionId)
                .AsNoTracking()
                .Where(o => o.BranchId == null && o.IsActive)
                .ToListAsync();
            configurationSource = "Organization";
            resolvedBranchId = null;
            resolvedBranchName = null;
        }

        var sortedDays = records
            .Select(o => o.DayOfWeek)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        return new OffDayScheduleDto
        {
            BranchId = resolvedBranchId,
            BranchName = resolvedBranchName,
            ConfigurationSource = configurationSource,
            OffDays = sortedDays,
            OffDayNames = sortedDays.Select(GetDayName).ToList(),
            TotalOffDaysPerWeek = sortedDays.Count
        };
    }

    public async Task<bool> IsOffDayAsync(DateTime date, int? branchId = null)
    {
        var schedule = await GetResolvedScheduleAsync(branchId);
        return schedule.OffDays.Contains((int)date.DayOfWeek);
    }

    public async Task<OffDayResponseDto> UpdateAsync(int id, UpdateOffDayDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var offDay = await _offDayRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Off day with ID {id} not found.");

        if (offDay.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this off day.");
        }

        offDay.IsActive = dto.IsActive;
        offDay.UpdatedAt = DateTime.UtcNow;

        await _offDayRepository.UpdateAsync(offDay);

        return await LoadResponseAsync(offDay.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var offDay = await _offDayRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Off day with ID {id} not found.");

        if (offDay.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this off day.");
        }

        await _offDayRepository.DeleteAsync(offDay);
    }

    private IQueryable<OffDay> BaseQuery(int subscriptionId)
    {
        return _offDayRepository
            .Query()
            .Include(o => o.Branch)
            .Where(o => o.SubscriptionId == subscriptionId);
    }

    private async Task<OffDayResponseDto> LoadResponseAsync(int offDayId, int subscriptionId)
    {
        var offDay = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offDayId);

        if (offDay is null)
        {
            var existsForOtherTenant = await _offDayRepository.Query()
                .AnyAsync(o => o.Id == offDayId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this off day.");
            }

            throw new KeyNotFoundException($"Off day with ID {offDayId} not found.");
        }

        return MapToResponseDto(offDay);
    }

    private OffDayResponseDto MapToResponseDto(OffDay o)
    {
        var dto = _mapper.Map<OffDayResponseDto>(o);
        dto.IsOrganizationWide = !o.BranchId.HasValue;
        dto.BranchName = o.Branch?.Name;
        return dto;
    }

    private async Task EnsureDayUniqueAsync(
        int dayOfWeek, int? branchId, int subscriptionId, int? excludeId = null)
    {
        var exists = await _offDayRepository.Query()
            .AnyAsync(o =>
                o.DayOfWeek == dayOfWeek &&
                o.BranchId == branchId &&
                o.SubscriptionId == subscriptionId &&
                o.IsActive &&
                (excludeId == null || o.Id != excludeId));

        if (exists)
        {
            var scope = branchId.HasValue ? $"Branch {branchId}" : "organization";
            throw new InvalidOperationException(
                $"{GetDayName(dayOfWeek)} is already configured as an off day for this {scope}.");
        }
    }

    private async Task ValidateBranchAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
        }
    }

    private static void ValidateDaysOfWeek(IEnumerable<int> days)
    {
        var list = days.ToList();

        var invalid = list.Where(d => d < 0 || d > 6).ToList();
        if (invalid.Count > 0)
        {
            throw new InvalidOperationException(
                $"Invalid DayOfWeek values: {string.Join(", ", invalid)}. " +
                "Accepted range is 0 (Sunday) to 6 (Saturday).");
        }

        var duplicates = list.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate days in request: {string.Join(", ", duplicates.Select(GetDayName))}.");
        }
    }

    private static string GetDayName(int dayOfWeek) =>
        ((DayOfWeek)dayOfWeek).ToString();

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

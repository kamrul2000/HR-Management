using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.HolidayCalendar;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class HolidayCalendarService : IHolidayCalendarService
{
    private const int MaxBulkBatchSize = 100;

    private static readonly string[] ValidHolidayTypes =
        { "Public", "Optional", "Organizational" };

    private readonly IRepository<HolidayCalendar> _holidayRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public HolidayCalendarService(
        IRepository<HolidayCalendar> holidayRepository,
        IRepository<Branch> branchRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _holidayRepository = holidayRepository;
        _branchRepository = branchRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<HolidayResponseDto> CreateAsync(CreateHolidayDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        ValidateHolidayType(dto.HolidayType);

        if (dto.BranchId.HasValue)
        {
            await ValidateBranchAsync(dto.BranchId.Value, subscriptionId);
        }

        await EnsureDateUniqueAsync(dto.HolidayDate, dto.BranchId, subscriptionId);

        var now = DateTime.UtcNow;
        var holiday = _mapper.Map<HolidayCalendar>(dto);
        holiday.HolidayDate = NormalizeDate(dto.HolidayDate);
        holiday.SubscriptionId = subscriptionId;
        holiday.IsActive = true;
        holiday.CreatedAt = now;
        holiday.UpdatedAt = now;

        await _holidayRepository.AddAsync(holiday);

        return await LoadResponseAsync(holiday.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkCreateAsync(BulkCreateHolidayDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        if (dto.Holidays is null || dto.Holidays.Count == 0)
        {
            throw new InvalidOperationException("At least one holiday must be provided.");
        }

        if (dto.Holidays.Count > MaxBulkBatchSize)
        {
            throw new InvalidOperationException(
                $"Bulk create cannot exceed {MaxBulkBatchSize} holidays per request.");
        }

        var result = new BulkCreateResultDto();

        foreach (var item in dto.Holidays)
        {
            try
            {
                ValidateHolidayType(item.HolidayType);

                if (item.BranchId.HasValue)
                {
                    await ValidateBranchAsync(item.BranchId.Value, subscriptionId);
                }

                await EnsureDateUniqueAsync(item.HolidayDate, item.BranchId, subscriptionId);

                var now = DateTime.UtcNow;
                var holiday = new HolidayCalendar
                {
                    HolidayName = item.HolidayName,
                    HolidayDate = NormalizeDate(item.HolidayDate),
                    HolidayType = item.HolidayType,
                    Description = item.Description,
                    IsRecurringYearly = item.IsRecurringYearly,
                    BranchId = item.BranchId,
                    SubscriptionId = subscriptionId,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _holidayRepository.AddAsync(holiday);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.FailedReasons.Add(
                    $"{item.HolidayName} ({NormalizeDate(item.HolidayDate):yyyy-MM-dd}): {ex.Message}");
            }
        }

        return result;
    }

    public async Task<HolidayResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<HolidayResponseDto>> GetFilteredAsync(HolidayFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.Year.HasValue)
        {
            var year = filter.Year.Value;
            query = query.Where(h => h.HolidayDate.Year == year);

            if (filter.Month.HasValue)
            {
                var month = filter.Month.Value;
                query = query.Where(h => h.HolidayDate.Month == month);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.HolidayType))
        {
            var type = filter.HolidayType.Trim();
            query = query.Where(h => h.HolidayType == type);
        }

        if (filter.BranchId is int branchId)
        {
            if (branchId == 0)
            {
                query = query.Where(h => h.BranchId == null);
            }
            else
            {
                query = query.Where(h => h.BranchId == branchId || h.BranchId == null);
            }
        }

        if (!filter.IncludeInactive)
        {
            query = query.Where(h => h.IsActive);
        }

        var items = await query
            .OrderBy(h => h.HolidayDate)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public Task<IEnumerable<HolidayResponseDto>> GetByYearAsync(int year, int? branchId = null)
    {
        return GetFilteredAsync(new HolidayFilterDto
        {
            Year = year,
            BranchId = branchId
        });
    }

    public async Task<HolidayCheckResultDto> IsHolidayAsync(DateTime date, int? branchId = null)
    {
        var subscriptionId = GetSubscriptionId();
        var normalizedDate = NormalizeDate(date);

        var match = await _holidayRepository.Query()
            .AsNoTracking()
            .Where(h =>
                h.SubscriptionId == subscriptionId &&
                h.IsActive &&
                h.HolidayDate == normalizedDate &&
                (h.BranchId == null || (branchId != null && h.BranchId == branchId)))
            .OrderBy(h => h.BranchId.HasValue ? 0 : 1)
            .FirstOrDefaultAsync();

        if (match is null)
        {
            return new HolidayCheckResultDto { IsHoliday = false };
        }

        return new HolidayCheckResultDto
        {
            IsHoliday = true,
            HolidayName = match.HolidayName,
            HolidayType = match.HolidayType
        };
    }

    public async Task<HolidayResponseDto> UpdateAsync(int id, UpdateHolidayDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var holiday = await _holidayRepository.Query()
            .FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"Holiday with ID {id} not found.");

        if (holiday.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this holiday.");
        }

        ValidateHolidayType(dto.HolidayType);

        if (dto.BranchId.HasValue && dto.BranchId != holiday.BranchId)
        {
            await ValidateBranchAsync(dto.BranchId.Value, subscriptionId);
        }

        var dateChanged = NormalizeDate(dto.HolidayDate) != holiday.HolidayDate;
        var branchChanged = dto.BranchId != holiday.BranchId;

        if (dateChanged || branchChanged)
        {
            await EnsureDateUniqueAsync(dto.HolidayDate, dto.BranchId, subscriptionId, excludeId: id);
        }

        holiday.HolidayName = dto.HolidayName;
        holiday.HolidayDate = NormalizeDate(dto.HolidayDate);
        holiday.HolidayType = dto.HolidayType;
        holiday.Description = dto.Description;
        holiday.IsRecurringYearly = dto.IsRecurringYearly;
        holiday.BranchId = dto.BranchId;
        holiday.IsActive = dto.IsActive;
        holiday.UpdatedAt = DateTime.UtcNow;

        await _holidayRepository.UpdateAsync(holiday);

        return await LoadResponseAsync(holiday.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var holiday = await _holidayRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Holiday with ID {id} not found.");

        if (holiday.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this holiday.");
        }

        await _holidayRepository.DeleteAsync(holiday);
    }

    private IQueryable<HolidayCalendar> BaseQuery(int subscriptionId)
    {
        return _holidayRepository
            .Query()
            .Include(h => h.Branch)
            .Where(h => h.SubscriptionId == subscriptionId);
    }

    private async Task<HolidayResponseDto> LoadResponseAsync(int holidayId, int subscriptionId)
    {
        var holiday = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == holidayId);

        if (holiday is null)
        {
            var existsForOtherTenant = await _holidayRepository.Query()
                .AnyAsync(h => h.Id == holidayId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this holiday.");
            }

            throw new KeyNotFoundException($"Holiday with ID {holidayId} not found.");
        }

        return MapToResponseDto(holiday);
    }

    private HolidayResponseDto MapToResponseDto(HolidayCalendar h)
    {
        var dto = _mapper.Map<HolidayResponseDto>(h);

        dto.HolidayDateFormatted = h.HolidayDate.ToString("dddd, dd MMMM yyyy");

        dto.HolidayTypeLabel = h.HolidayType switch
        {
            "Public" => "Public Holiday",
            "Optional" => "Optional Holiday",
            "Organizational" => "Organizational Holiday",
            _ => h.HolidayType
        };

        dto.IsOrganizationWide = !h.BranchId.HasValue;
        dto.BranchName = h.Branch?.Name;

        return dto;
    }

    private async Task EnsureDateUniqueAsync(
        DateTime date, int? branchId, int subscriptionId, int? excludeId = null)
    {
        var normalizedDate = NormalizeDate(date);

        var exists = await _holidayRepository.Query()
            .AnyAsync(h =>
                h.HolidayDate == normalizedDate &&
                h.BranchId == branchId &&
                h.SubscriptionId == subscriptionId &&
                h.IsActive &&
                (excludeId == null || h.Id != excludeId));

        if (exists)
        {
            var scope = branchId.HasValue ? $"Branch {branchId}" : "organization-wide";
            throw new InvalidOperationException(
                $"A holiday already exists on {normalizedDate:dd MMM yyyy} for {scope}.");
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

    private static void ValidateHolidayType(string holidayType)
    {
        if (!ValidHolidayTypes.Contains(holidayType))
        {
            throw new InvalidOperationException(
                $"Invalid holiday type '{holidayType}'. " +
                $"Accepted values: {string.Join(", ", ValidHolidayTypes)}.");
        }
    }

    private static DateTime NormalizeDate(DateTime date) => date.Date;

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

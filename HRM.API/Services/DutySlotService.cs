using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.DutySlot;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class DutySlotService : IDutySlotService
{
    private readonly IRepository<DutySlot> _dutySlotRepository;
    private readonly IRepository<Attendance> _attendanceRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public DutySlotService(
        IRepository<DutySlot> dutySlotRepository,
        IRepository<Attendance> attendanceRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _dutySlotRepository = dutySlotRepository;
        _attendanceRepository = attendanceRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<DutySlotResponseDto> CreateAsync(CreateDutySlotDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureSlotNameUniqueAsync(dto.SlotName.Trim(), subscriptionId);

        var (totalWorkingHours, isNightShift) = ComputeShiftMetrics(
            dto.StartTime, dto.EndTime, dto.BreakDurationMinutes);

        var now = DateTime.UtcNow;
        var dutySlot = _mapper.Map<DutySlot>(dto);
        dutySlot.SlotName = dto.SlotName.Trim();
        dutySlot.TotalWorkingHours = totalWorkingHours;
        dutySlot.IsNightShift = isNightShift;
        dutySlot.SubscriptionId = subscriptionId;
        dutySlot.IsActive = true;
        dutySlot.CreatedAt = now;
        dutySlot.UpdatedAt = now;

        await _dutySlotRepository.AddAsync(dutySlot);

        return MapToResponseDto(dutySlot);
    }

    public async Task<DutySlotResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var dutySlot = await _dutySlotRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Duty slot with ID {id} not found.");

        if (dutySlot.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this duty slot.");
        }

        return MapToResponseDto(dutySlot);
    }

    public async Task<IEnumerable<DutySlotResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var dutySlots = await _dutySlotRepository.Query()
            .AsNoTracking()
            .Where(s => s.SubscriptionId == subscriptionId)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.SlotName)
            .ToListAsync();

        return dutySlots.Select(MapToResponseDto).ToList();
    }

    public async Task<DutySlotResponseDto> UpdateAsync(int id, UpdateDutySlotDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var dutySlot = await _dutySlotRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Duty slot with ID {id} not found.");

        if (dutySlot.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this duty slot.");
        }

        var trimmedName = dto.SlotName.Trim();
        if (!string.Equals(trimmedName, dutySlot.SlotName, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureSlotNameUniqueAsync(trimmedName, subscriptionId, excludeId: id);
        }

        var (totalWorkingHours, isNightShift) = ComputeShiftMetrics(
            dto.StartTime, dto.EndTime, dto.BreakDurationMinutes);

        dutySlot.SlotName = trimmedName;
        dutySlot.StartTime = dto.StartTime;
        dutySlot.EndTime = dto.EndTime;
        dutySlot.BreakDurationMinutes = dto.BreakDurationMinutes;
        dutySlot.LateToleranceMinutes = dto.LateToleranceMinutes;
        dutySlot.TotalWorkingHours = totalWorkingHours;
        dutySlot.IsNightShift = isNightShift;
        dutySlot.IsActive = dto.IsActive;
        dutySlot.UpdatedAt = DateTime.UtcNow;

        await _dutySlotRepository.UpdateAsync(dutySlot);

        return MapToResponseDto(dutySlot);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var dutySlot = await _dutySlotRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Duty slot with ID {id} not found.");

        if (dutySlot.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this duty slot.");
        }

        var inUse = await _attendanceRepository.Query()
            .AnyAsync(a => a.DutySlotId == id);

        if (inUse)
        {
            throw new InvalidOperationException("Cannot delete a duty slot that has been used in attendance records.");
        }

        await _dutySlotRepository.DeleteAsync(dutySlot);
    }

    private DutySlotResponseDto MapToResponseDto(DutySlot dutySlot)
    {
        var dto = _mapper.Map<DutySlotResponseDto>(dutySlot);
        dto.StartTimeFormatted = DateTime.Today.Add(dutySlot.StartTime).ToString("hh:mm tt");
        dto.EndTimeFormatted = DateTime.Today.Add(dutySlot.EndTime).ToString("hh:mm tt");
        dto.TotalWorkingHoursFormatted = FormatWorkingHours(dutySlot.TotalWorkingHours);
        return dto;
    }

    private async Task EnsureSlotNameUniqueAsync(string slotName, int subscriptionId, int? excludeId = null)
    {
        var exists = await _dutySlotRepository
            .Query()
            .AnyAsync(s =>
                s.SlotName == slotName &&
                s.SubscriptionId == subscriptionId &&
                (excludeId == null || s.Id != excludeId));

        if (exists)
        {
            throw new InvalidOperationException(
                $"A duty slot named '{slotName}' already exists for this organization.");
        }
    }

    private static (decimal totalWorkingHours, bool isNightShift) ComputeShiftMetrics(
        TimeSpan startTime, TimeSpan endTime, int breakDurationMinutes)
    {
        bool isNightShift = endTime < startTime;

        double grossMinutes = isNightShift
            ? (TimeSpan.FromHours(24) - startTime + endTime).TotalMinutes
            : (endTime - startTime).TotalMinutes;

        double netMinutes = grossMinutes - breakDurationMinutes;

        if (netMinutes < 30)
        {
            throw new InvalidOperationException(
                "Total working time after break must be at least 30 minutes.");
        }

        decimal totalWorkingHours = Math.Round((decimal)(netMinutes / 60.0), 2);
        return (totalWorkingHours, isNightShift);
    }

    private static string FormatWorkingHours(decimal totalHours)
    {
        int hours = (int)totalHours;
        int minutes = (int)Math.Round((totalHours - hours) * 60);
        return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

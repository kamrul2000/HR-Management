using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LeaveType;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly IRepository<LeaveType> _leaveTypeRepository;
    private readonly IRepository<LeaveAllotment> _leaveAllotmentRepository;
    private readonly IRepository<LeaveApplication> _leaveApplicationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public LeaveTypeService(
        IRepository<LeaveType> leaveTypeRepository,
        IRepository<LeaveAllotment> leaveAllotmentRepository,
        IRepository<LeaveApplication> leaveApplicationRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _leaveAllotmentRepository = leaveAllotmentRepository;
        _leaveApplicationRepository = leaveApplicationRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<LeaveTypeResponseDto> CreateAsync(CreateLeaveTypeDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        ValidateLeaveTypeRules(dto.IsCarryForward, dto.MaxCarryForwardDays, dto.GenderRestriction);

        var name = dto.Name.Trim();
        var code = dto.Code.Trim().ToUpperInvariant();

        await EnsureUniqueAsync(name, code, subscriptionId);

        var now = DateTime.UtcNow;
        var leaveType = _mapper.Map<LeaveType>(dto);
        leaveType.Name = name;
        leaveType.Code = code;
        leaveType.SubscriptionId = subscriptionId;
        leaveType.IsActive = true;
        leaveType.CreatedAt = now;
        leaveType.UpdatedAt = now;

        await _leaveTypeRepository.AddAsync(leaveType);

        return MapToResponseDto(leaveType);
    }

    public async Task<LeaveTypeResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var leaveType = await _leaveTypeRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave type with ID {id} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }

        return MapToResponseDto(leaveType);
    }

    public async Task<IEnumerable<LeaveTypeResponseDto>> GetAllAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var leaveTypes = await _leaveTypeRepository.Query()
            .AsNoTracking()
            .Where(l => l.SubscriptionId == subscriptionId)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return leaveTypes.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<LeaveTypeResponseDto>> GetActiveAsync()
    {
        var subscriptionId = GetSubscriptionId();

        var leaveTypes = await _leaveTypeRepository.Query()
            .AsNoTracking()
            .Where(l => l.SubscriptionId == subscriptionId && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return leaveTypes.Select(MapToResponseDto).ToList();
    }

    public async Task<LeaveTypeResponseDto> UpdateAsync(int id, UpdateLeaveTypeDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var leaveType = await _leaveTypeRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave type with ID {id} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }

        ValidateLeaveTypeRules(dto.IsCarryForward, dto.MaxCarryForwardDays, dto.GenderRestriction);

        var name = dto.Name.Trim();
        var code = dto.Code.Trim().ToUpperInvariant();

        var nameChanged = !string.Equals(name, leaveType.Name, StringComparison.OrdinalIgnoreCase);
        var codeChanged = !string.Equals(code, leaveType.Code, StringComparison.OrdinalIgnoreCase);

        if (nameChanged || codeChanged)
        {
            await EnsureUniqueAsync(name, code, subscriptionId, excludeId: id);
        }

        leaveType.Name = name;
        leaveType.Code = code;
        leaveType.Description = dto.Description;
        leaveType.IsPaid = dto.IsPaid;
        leaveType.IsCarryForward = dto.IsCarryForward;
        leaveType.MaxCarryForwardDays = dto.MaxCarryForwardDays;
        leaveType.RequiresApproval = dto.RequiresApproval;
        leaveType.RequiresDocument = dto.RequiresDocument;
        leaveType.MinNoticeDays = dto.MinNoticeDays;
        leaveType.MaxConsecutiveDays = dto.MaxConsecutiveDays;
        leaveType.GenderRestriction = dto.GenderRestriction;
        leaveType.IsActive = dto.IsActive;
        leaveType.UpdatedAt = DateTime.UtcNow;

        await _leaveTypeRepository.UpdateAsync(leaveType);

        return MapToResponseDto(leaveType);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var leaveType = await _leaveTypeRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave type with ID {id} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }

        var hasAllotments = await _leaveAllotmentRepository.Query()
            .AnyAsync(a => a.LeaveTypeId == id);

        if (hasAllotments)
        {
            throw new InvalidOperationException(
                "Cannot delete a leave type with existing allotments. Remove all allotments first.");
        }

        var hasApplications = await _leaveApplicationRepository.Query()
            .AnyAsync(a => a.LeaveTypeId == id);

        if (hasApplications)
        {
            throw new InvalidOperationException(
                "Cannot delete a leave type with existing applications.");
        }

        await _leaveTypeRepository.DeleteAsync(leaveType);
    }

    private LeaveTypeResponseDto MapToResponseDto(LeaveType leaveType)
    {
        var dto = _mapper.Map<LeaveTypeResponseDto>(leaveType);

        dto.IsPaidLabel = leaveType.IsPaid ? "Paid" : "Unpaid";

        dto.GenderRestrictionLabel = leaveType.GenderRestriction switch
        {
            "Male" => "Male Only",
            "Female" => "Female Only",
            _ => "All Genders"
        };

        return dto;
    }

    private async Task EnsureUniqueAsync(string name, string code, int subscriptionId, int? excludeId = null)
    {
        var nameExists = await _leaveTypeRepository.Query()
            .AnyAsync(l =>
                l.Name == name &&
                l.SubscriptionId == subscriptionId &&
                (excludeId == null || l.Id != excludeId));

        if (nameExists)
        {
            throw new InvalidOperationException($"A leave type named '{name}' already exists.");
        }

        var codeExists = await _leaveTypeRepository.Query()
            .AnyAsync(l =>
                l.Code == code &&
                l.SubscriptionId == subscriptionId &&
                (excludeId == null || l.Id != excludeId));

        if (codeExists)
        {
            throw new InvalidOperationException($"A leave type with code '{code}' already exists.");
        }
    }

    private static void ValidateLeaveTypeRules(
        bool isCarryForward, int maxCarryForwardDays, string? genderRestriction)
    {
        if (!isCarryForward && maxCarryForwardDays > 0)
        {
            throw new InvalidOperationException(
                "MaxCarryForwardDays must be 0 when IsCarryForward is false.");
        }

        if (genderRestriction is not null &&
            genderRestriction != "Male" &&
            genderRestriction != "Female")
        {
            throw new InvalidOperationException(
                "GenderRestriction must be null, 'Male', or 'Female'.");
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

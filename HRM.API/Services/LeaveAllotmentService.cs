using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class LeaveAllotmentService : ILeaveAllotmentService
{
    private const int MaxBulkBatchSize = 500;

    private readonly IRepository<LeaveAllotment> _allotmentRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<LeaveType> _leaveTypeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public LeaveAllotmentService(
        IRepository<LeaveAllotment> allotmentRepository,
        IRepository<Employee> employeeRepository,
        IRepository<LeaveType> leaveTypeRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _allotmentRepository = allotmentRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<LeaveAllotmentResponseDto> CreateAsync(CreateLeaveAllotmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var (_, leaveType) = await ResolveAndValidateAsync(dto.EmployeeId, dto.LeaveTypeId, subscriptionId);

        ValidateCarryForward(dto.CarriedForwardDays, leaveType);

        var duplicateExists = await _allotmentRepository.Query()
            .AnyAsync(a =>
                a.EmployeeId == dto.EmployeeId &&
                a.LeaveTypeId == dto.LeaveTypeId &&
                a.Year == dto.Year &&
                a.SubscriptionId == subscriptionId);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"An allotment for '{leaveType.Name}' in year {dto.Year} already exists for this employee.");
        }

        var now = DateTime.UtcNow;
        var allotment = _mapper.Map<LeaveAllotment>(dto);
        allotment.UsedDays = 0;
        allotment.SubscriptionId = subscriptionId;
        allotment.IsActive = true;
        allotment.CreatedAt = now;
        allotment.UpdatedAt = now;

        await _allotmentRepository.AddAsync(allotment);

        return await LoadResponseAsync(allotment.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkCreateAsync(BulkCreateLeaveAllotmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        if (dto.EmployeeIds is null || dto.EmployeeIds.Count == 0)
        {
            throw new InvalidOperationException("At least one employee ID must be provided.");
        }

        if (dto.EmployeeIds.Count > MaxBulkBatchSize)
        {
            throw new InvalidOperationException($"Bulk allotment cannot exceed {MaxBulkBatchSize} employees per request.");
        }

        var leaveType = await _leaveTypeRepository.GetByIdAsync(dto.LeaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType with ID {dto.LeaveTypeId} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }

        if (!leaveType.IsActive)
        {
            throw new InvalidOperationException("Cannot allot an inactive leave type.");
        }

        var result = new BulkCreateResultDto();
        var distinctEmployeeIds = dto.EmployeeIds.Distinct().ToList();

        foreach (var employeeId in distinctEmployeeIds)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);

                if (employee is null || employee.SubscriptionId != subscriptionId)
                {
                    result.FailedCount++;
                    result.FailedReasons.Add($"Employee {employeeId}: not found or access denied.");
                    continue;
                }

                if (!employee.IsActive)
                {
                    result.SkippedCount++;
                    result.SkippedReasons.Add($"Employee {employee.EmployeeCode}: inactive.");
                    continue;
                }

                var alreadyAllotted = await _allotmentRepository.Query()
                    .AnyAsync(a =>
                        a.EmployeeId == employeeId &&
                        a.LeaveTypeId == dto.LeaveTypeId &&
                        a.Year == dto.Year &&
                        a.SubscriptionId == subscriptionId);

                if (alreadyAllotted)
                {
                    result.SkippedCount++;
                    result.SkippedReasons.Add($"Employee {employee.EmployeeCode}: already allotted for {dto.Year}.");
                    continue;
                }

                ValidateCarryForward(0m, leaveType);

                var now = DateTime.UtcNow;
                var allotment = new LeaveAllotment
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = dto.LeaveTypeId,
                    Year = dto.Year,
                    AllocatedDays = dto.AllocatedDays,
                    UsedDays = 0,
                    CarriedForwardDays = 0,
                    SubscriptionId = subscriptionId,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _allotmentRepository.AddAsync(allotment);
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

    public async Task<LeaveAllotmentResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<IEnumerable<LeaveAllotmentResponseDto>> GetByEmployeeAsync(int employeeId, int? year = null)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureEmployeeOwnershipAsync(employeeId, subscriptionId);

        var query = BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId);

        if (year.HasValue)
        {
            query = query.Where(a => a.Year == year.Value);
        }

        var items = await query
            .OrderByDescending(a => a.Year)
            .ThenBy(a => a.LeaveType.Name)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<LeaveAllotmentResponseDto>> GetByLeaveTypeAsync(int leaveTypeId, int year)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureLeaveTypeOwnershipAsync(leaveTypeId, subscriptionId);

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(a => a.LeaveTypeId == leaveTypeId && a.Year == year)
            .OrderBy(a => a.Employee.FullName)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<IEnumerable<LeaveAllotmentResponseDto>> GetByYearAsync(int year)
    {
        var subscriptionId = GetSubscriptionId();

        var items = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .Where(a => a.Year == year)
            .OrderBy(a => a.Employee.FullName)
            .ThenBy(a => a.LeaveType.Name)
            .ToListAsync();

        return items.Select(MapToResponseDto).ToList();
    }

    public async Task<LeaveAllotmentResponseDto> UpdateAsync(int id, UpdateLeaveAllotmentDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var allotment = await _allotmentRepository.Query()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Leave allotment with ID {id} not found.");

        if (allotment.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave allotment.");
        }

        var leaveType = await _leaveTypeRepository.GetByIdAsync(allotment.LeaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType with ID {allotment.LeaveTypeId} not found.");

        ValidateCarryForward(dto.CarriedForwardDays, leaveType);

        if (dto.AllocatedDays < allotment.UsedDays)
        {
            throw new InvalidOperationException(
                $"Allocated days ({dto.AllocatedDays}) cannot be less than already used days ({allotment.UsedDays}).");
        }

        allotment.AllocatedDays = dto.AllocatedDays;
        allotment.CarriedForwardDays = dto.CarriedForwardDays;
        allotment.IsActive = dto.IsActive;
        allotment.UpdatedAt = DateTime.UtcNow;

        await _allotmentRepository.UpdateAsync(allotment);

        return await LoadResponseAsync(allotment.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var allotment = await _allotmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave allotment with ID {id} not found.");

        if (allotment.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave allotment.");
        }

        if (allotment.UsedDays > 0)
        {
            throw new InvalidOperationException("Cannot delete an allotment that has been partially or fully used.");
        }

        await _allotmentRepository.DeleteAsync(allotment);
    }

    private IQueryable<LeaveAllotment> BaseQuery(int subscriptionId)
    {
        return _allotmentRepository
            .Query()
            .Include(a => a.Employee)
            .Include(a => a.LeaveType)
            .Where(a => a.SubscriptionId == subscriptionId);
    }

    private async Task<LeaveAllotmentResponseDto> LoadResponseAsync(int allotmentId, int subscriptionId)
    {
        var allotment = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == allotmentId);

        if (allotment is null)
        {
            var existsForOtherTenant = await _allotmentRepository.Query()
                .AnyAsync(a => a.Id == allotmentId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this leave allotment.");
            }

            throw new KeyNotFoundException($"Leave allotment with ID {allotmentId} not found.");
        }

        return MapToResponseDto(allotment);
    }

    private LeaveAllotmentResponseDto MapToResponseDto(LeaveAllotment a)
    {
        var dto = _mapper.Map<LeaveAllotmentResponseDto>(a);
        dto.RemainingDays = a.AllocatedDays + a.CarriedForwardDays - a.UsedDays;
        return dto;
    }

    private async Task<(Employee employee, LeaveType leaveType)> ResolveAndValidateAsync(
        int employeeId, int leaveTypeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        if (!employee.IsActive)
        {
            throw new InvalidOperationException("Cannot allot leave to an inactive employee.");
        }

        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType with ID {leaveTypeId} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }

        if (!leaveType.IsActive)
        {
            throw new InvalidOperationException("Cannot allot an inactive leave type.");
        }

        return (employee, leaveType);
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

    private async Task EnsureLeaveTypeOwnershipAsync(int leaveTypeId, int subscriptionId)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(leaveTypeId)
            ?? throw new KeyNotFoundException($"LeaveType with ID {leaveTypeId} not found.");

        if (leaveType.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this leave type.");
        }
    }

    private static void ValidateCarryForward(decimal carriedForwardDays, LeaveType leaveType)
    {
        if (carriedForwardDays > 0 && !leaveType.IsCarryForward)
        {
            throw new InvalidOperationException(
                $"Leave type '{leaveType.Name}' does not support carry-forward.");
        }

        if (carriedForwardDays > leaveType.MaxCarryForwardDays)
        {
            throw new InvalidOperationException(
                $"Carried forward days ({carriedForwardDays}) exceeds the maximum " +
                $"allowed ({leaveType.MaxCarryForwardDays}) for '{leaveType.Name}'.");
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

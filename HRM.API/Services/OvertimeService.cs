using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.Overtime;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class OvertimeService : IOvertimeService
{
    private const int MaxPageSize = 100;

    private readonly IRepository<Overtime> _overtimeRepository;
    private readonly IRepository<Attendance> _attendanceRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public OvertimeService(
        IRepository<Overtime> overtimeRepository,
        IRepository<Attendance> attendanceRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Branch> branchRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _overtimeRepository = overtimeRepository;
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<OvertimeResponseDto> CreateAsync(CreateOvertimeDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);

        var attendance = await _attendanceRepository.Query()
            .FirstOrDefaultAsync(a => a.Id == dto.AttendanceId && a.SubscriptionId == subscriptionId)
            ?? throw new KeyNotFoundException($"Attendance with ID {dto.AttendanceId} not found.");

        if (attendance.EmployeeId != dto.EmployeeId)
        {
            throw new InvalidOperationException("Attendance record does not belong to this employee.");
        }

        var overtimeType = ResolveOvertimeType(attendance.Status);
        ValidateOvertimeMinutes(dto.RequestedMinutes, attendance);

        var duplicate = await _overtimeRepository.Query()
            .AnyAsync(o => o.AttendanceId == dto.AttendanceId && o.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "An overtime record already exists for this attendance entry.");
        }

        var now = DateTime.UtcNow;
        var overtime = new Overtime
        {
            EmployeeId = employee.Id,
            AttendanceId = attendance.Id,
            OvertimeDate = attendance.AttendanceDate,
            RequestedMinutes = dto.RequestedMinutes,
            ApprovedMinutes = 0,
            OvertimeType = overtimeType,
            Reason = dto.Reason,
            Status = "Pending",
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _overtimeRepository.AddAsync(overtime);

        return await LoadResponseAsync(overtime.Id, subscriptionId);
    }

    public async Task<OvertimeResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<OvertimeResponseDto>> GetFilteredAsync(OvertimeFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(o => o.EmployeeId == empId);
        }

        if (filter.BranchId is int brId)
        {
            query = query.Where(o => o.Employee.BranchId == brId);
        }

        if (filter.DepartmentId is int deptId)
        {
            query = query.Where(o => o.Employee.DepartmentId == deptId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.OvertimeType))
        {
            var type = filter.OvertimeType.Trim();
            query = query.Where(o => o.OvertimeType == type);
        }

        if (filter.FromDate is DateTime from)
        {
            var fromDate = from.Date;
            query = query.Where(o => o.OvertimeDate >= fromDate);
        }

        if (filter.ToDate is DateTime to)
        {
            var toDate = to.Date;
            query = query.Where(o => o.OvertimeDate <= toDate);
        }

        if (filter.Year is int year && filter.Month is int month)
        {
            query = query.Where(o => o.OvertimeDate.Year == year && o.OvertimeDate.Month == month);
        }
        else if (filter.Year is int yearOnly)
        {
            query = query.Where(o => o.OvertimeDate.Year == yearOnly);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.OvertimeDate)
            .ThenBy(o => o.Employee.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<OvertimeResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<OvertimeSummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeForReadAsync(employeeId, subscriptionId);

        var firstOfMonth = new DateTime(year, month, 1);
        var lastOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var records = await _overtimeRepository.Query()
            .AsNoTracking()
            .Where(o =>
                o.EmployeeId == employeeId &&
                o.SubscriptionId == subscriptionId &&
                o.OvertimeDate >= firstOfMonth &&
                o.OvertimeDate <= lastOfMonth)
            .ToListAsync();

        var totalRequested = records.Sum(r => r.RequestedMinutes);
        var approvedRecords = records.Where(r => r.Status == "Approved").ToList();
        var totalApproved = approvedRecords.Sum(r => r.ApprovedMinutes);
        var regularApproved = approvedRecords
            .Where(r => r.OvertimeType == "Regular").Sum(r => r.ApprovedMinutes);
        var holidayApproved = approvedRecords
            .Where(r => r.OvertimeType == "Holiday").Sum(r => r.ApprovedMinutes);
        var weeklyOffApproved = approvedRecords
            .Where(r => r.OvertimeType == "WeeklyOff").Sum(r => r.ApprovedMinutes);

        return new OvertimeSummaryDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeFullName = employee.FullName,
            Year = year,
            Month = month,
            TotalRequestedMinutes = totalRequested,
            TotalApprovedMinutes = totalApproved,
            RegularOvertimeMinutes = regularApproved,
            HolidayOvertimeMinutes = holidayApproved,
            WeeklyOffOvertimeMinutes = weeklyOffApproved,
            TotalApprovedRecords = approvedRecords.Count,
            TotalApprovedFormatted = FormatMinutes(totalApproved)
        };
    }

    public async Task<IEnumerable<OvertimeSummaryDto>> GetMonthlySummaryByBranchAsync(
        int branchId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureBranchOwnershipAsync(branchId, subscriptionId);

        var employees = await _employeeRepository.Query()
            .AsNoTracking()
            .Where(e => e.BranchId == branchId && e.SubscriptionId == subscriptionId)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        var summaries = new List<OvertimeSummaryDto>();
        foreach (var employee in employees)
        {
            summaries.Add(await GetMonthlySummaryAsync(employee.Id, year, month));
        }

        return summaries;
    }

    public async Task<OvertimeResponseDto> ApproveAsync(int id, ApproveOvertimeDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var overtime = await _overtimeRepository.Query()
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Overtime with ID {id} not found.");

        if (overtime.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this overtime record.");
        }

        if (overtime.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending overtime records can be approved.");
        }

        var approvedMinutes = dto.ApprovedMinutes ?? overtime.RequestedMinutes;

        if (approvedMinutes < 1)
        {
            throw new InvalidOperationException("Approved minutes must be at least 1.");
        }

        if (approvedMinutes > overtime.RequestedMinutes)
        {
            throw new InvalidOperationException("Approved minutes cannot exceed requested minutes.");
        }

        var now = DateTime.UtcNow;
        overtime.Status = "Approved";
        overtime.ApprovedMinutes = approvedMinutes;
        overtime.ApprovedById = callerId;
        overtime.ApprovalDate = now;
        overtime.ApprovalRemarks = dto.ApprovalRemarks;
        overtime.UpdatedAt = now;

        await _overtimeRepository.UpdateAsync(overtime);

        return await LoadResponseAsync(overtime.Id, subscriptionId);
    }

    public async Task<OvertimeResponseDto> RejectAsync(int id, RejectOvertimeDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var callerId = GetCallerId();

        var overtime = await _overtimeRepository.Query()
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Overtime with ID {id} not found.");

        if (overtime.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this overtime record.");
        }

        if (overtime.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending overtime records can be rejected.");
        }

        var now = DateTime.UtcNow;
        overtime.Status = "Rejected";
        overtime.ApprovedMinutes = 0;
        overtime.ApprovedById = callerId;
        overtime.ApprovalDate = now;
        overtime.ApprovalRemarks = dto.ApprovalRemarks;
        overtime.UpdatedAt = now;

        await _overtimeRepository.UpdateAsync(overtime);

        return await LoadResponseAsync(overtime.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var overtime = await _overtimeRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Overtime with ID {id} not found.");

        if (overtime.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this overtime record.");
        }

        if (overtime.Status == "Approved")
        {
            throw new InvalidOperationException(
                "Approved overtime records cannot be deleted. They are referenced in salary calculations.");
        }

        await _overtimeRepository.DeleteAsync(overtime);
    }

    private IQueryable<Overtime> BaseQuery(int subscriptionId)
    {
        return _overtimeRepository
            .Query()
            .Include(o => o.Employee)
            .Include(o => o.Attendance)
            .Where(o => o.SubscriptionId == subscriptionId);
    }

    private async Task<OvertimeResponseDto> LoadResponseAsync(int overtimeId, int subscriptionId)
    {
        var overtime = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == overtimeId);

        if (overtime is null)
        {
            var existsForOtherTenant = await _overtimeRepository.Query()
                .AnyAsync(o => o.Id == overtimeId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this overtime record.");
            }

            throw new KeyNotFoundException($"Overtime with ID {overtimeId} not found.");
        }

        return MapToResponseDto(overtime);
    }

    private OvertimeResponseDto MapToResponseDto(Overtime o)
    {
        var dto = _mapper.Map<OvertimeResponseDto>(o);

        dto.OvertimeDateFormatted = o.OvertimeDate.ToString("dddd, dd MMM yyyy");
        dto.RequestedFormatted = FormatMinutes(o.RequestedMinutes);
        dto.ApprovedFormatted = o.ApprovedMinutes > 0 ? FormatMinutes(o.ApprovedMinutes) : "—";
        dto.ApprovalDateFormatted = o.ApprovalDate?.ToString("dd MMM yyyy");

        dto.OvertimeTypeLabel = o.OvertimeType switch
        {
            "Regular" => "Regular OT",
            "Holiday" => "Holiday OT",
            "WeeklyOff" => "Weekly Off OT",
            _ => o.OvertimeType
        };

        dto.StatusLabel = o.Status switch
        {
            "Pending" => "Pending Approval",
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            _ => o.Status
        };

        return dto;
    }

    private static string ResolveOvertimeType(string attendanceStatus) =>
        attendanceStatus switch
        {
            "Present" or "Late" => "Regular",
            "Holiday" => "Holiday",
            "WeeklyOff" => "WeeklyOff",
            "Absent" => throw new InvalidOperationException(
                "Overtime cannot be recorded for an absent employee."),
            "HalfDay" => throw new InvalidOperationException(
                "Overtime cannot be recorded for a half-day attendance."),
            _ => throw new InvalidOperationException(
                $"Unsupported attendance status '{attendanceStatus}' for overtime.")
        };

    private static void ValidateOvertimeMinutes(int requestedMinutes, Attendance attendance)
    {
        if (attendance.Status is "Present" or "Late" &&
            attendance.OvertimeMinutes > 0 &&
            requestedMinutes > attendance.OvertimeMinutes)
        {
            throw new InvalidOperationException(
                $"Requested overtime ({requestedMinutes} min) exceeds the recorded overtime " +
                $"in the attendance entry ({attendance.OvertimeMinutes} min). " +
                "Please correct the attendance record first.");
        }
    }

    private static string FormatMinutes(int totalMinutes)
    {
        int h = totalMinutes / 60;
        int m = totalMinutes % 60;
        return m > 0 ? $"{h}h {m}m" : $"{h}h";
    }

    private async Task<Employee> ResolveEmployeeAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        if (!employee.IsActive || employee.Status != "Active")
        {
            throw new InvalidOperationException(
                "Overtime can only be recorded for active employees.");
        }

        return employee;
    }

    private async Task<Employee> ResolveEmployeeForReadAsync(int employeeId, int subscriptionId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        if (employee.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this employee.");
        }

        return employee;
    }

    private async Task EnsureBranchOwnershipAsync(int branchId, int subscriptionId)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId)
            ?? throw new KeyNotFoundException($"Branch with ID {branchId} not found.");

        if (branch.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this branch.");
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

using AutoMapper;
using HRM.API.Helpers;
using HRM.API.Services.Interfaces;
using HRM.Core.DTOs.Attendance;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;
using HRM.Core.Entities;
using HRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public class AttendanceService : IAttendanceService
{
    private const int MaxPageSize = 200;

    private readonly IRepository<Attendance> _attendanceRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<DutySlot> _dutySlotRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly IRepository<Overtime> _overtimeRepository;
    private readonly IHolidayCalendarService _holidayService;
    private readonly IOffDayService _offDayService;
    private readonly IWorkingDayCalculator _workingDayCalculator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public AttendanceService(
        IRepository<Attendance> attendanceRepository,
        IRepository<Employee> employeeRepository,
        IRepository<DutySlot> dutySlotRepository,
        IRepository<Branch> branchRepository,
        IRepository<Overtime> overtimeRepository,
        IHolidayCalendarService holidayService,
        IOffDayService offDayService,
        IWorkingDayCalculator workingDayCalculator,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _dutySlotRepository = dutySlotRepository;
        _branchRepository = branchRepository;
        _overtimeRepository = overtimeRepository;
        _holidayService = holidayService;
        _offDayService = offDayService;
        _workingDayCalculator = workingDayCalculator;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<AttendanceResponseDto> CreateAsync(CreateAttendanceDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var date = dto.AttendanceDate.Date;

        var duplicate = await _attendanceRepository.Query()
            .AnyAsync(a =>
                a.EmployeeId == dto.EmployeeId &&
                a.AttendanceDate == date &&
                a.SubscriptionId == subscriptionId);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Attendance for this employee on {date:dd MMM yyyy} already exists.");
        }

        var employee = await ResolveEmployeeAsync(dto.EmployeeId, subscriptionId);
        var slot = await ResolveDutySlotAsync(dto.DutySlotId, subscriptionId);

        ValidatePunchTimes(dto.PunchInTime, dto.PunchOutTime, slot);

        var (status, isLate, lateMins, actualMins, overtimeMins) =
            await ComputeAttendanceMetricsAsync(date, dto.PunchInTime, dto.PunchOutTime, slot, employee.BranchId);

        var now = DateTime.UtcNow;
        var attendance = new Attendance
        {
            EmployeeId = employee.Id,
            DutySlotId = slot.Id,
            AttendanceDate = date,
            PunchInTime = dto.PunchInTime,
            PunchOutTime = dto.PunchOutTime,
            Status = status,
            IsLate = isLate,
            LateMinutes = lateMins,
            ActualWorkingMinutes = actualMins,
            ScheduledWorkingMinutes = (int)(slot.TotalWorkingHours * 60),
            OvertimeMinutes = overtimeMins,
            Remarks = dto.Remarks,
            SubscriptionId = subscriptionId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _attendanceRepository.AddAsync(attendance);

        return await LoadResponseAsync(attendance.Id, subscriptionId);
    }

    public async Task<BulkCreateResultDto> BulkCreateAsync(BulkAttendanceDto dto)
    {
        var subscriptionId = GetSubscriptionId();
        var date = dto.AttendanceDate.Date;
        var result = new BulkCreateResultDto();

        foreach (var entry in dto.Entries)
        {
            try
            {
                var duplicate = await _attendanceRepository.Query()
                    .AnyAsync(a =>
                        a.EmployeeId == entry.EmployeeId &&
                        a.AttendanceDate == date &&
                        a.SubscriptionId == subscriptionId);

                if (duplicate)
                {
                    result.SkippedCount++;
                    result.SkippedReasons.Add(
                        $"Employee {entry.EmployeeId}: attendance already recorded for {date:yyyy-MM-dd}.");
                    continue;
                }

                var employee = await ResolveEmployeeAsync(entry.EmployeeId, subscriptionId);
                var slot = await ResolveDutySlotAsync(entry.DutySlotId, subscriptionId);

                ValidatePunchTimes(entry.PunchInTime, entry.PunchOutTime, slot);

                var (status, isLate, lateMins, actualMins, overtimeMins) =
                    await ComputeAttendanceMetricsAsync(date, entry.PunchInTime, entry.PunchOutTime, slot, employee.BranchId);

                var now = DateTime.UtcNow;
                var attendance = new Attendance
                {
                    EmployeeId = employee.Id,
                    DutySlotId = slot.Id,
                    AttendanceDate = date,
                    PunchInTime = entry.PunchInTime,
                    PunchOutTime = entry.PunchOutTime,
                    Status = status,
                    IsLate = isLate,
                    LateMinutes = lateMins,
                    ActualWorkingMinutes = actualMins,
                    ScheduledWorkingMinutes = (int)(slot.TotalWorkingHours * 60),
                    OvertimeMinutes = overtimeMins,
                    Remarks = entry.Remarks,
                    SubscriptionId = subscriptionId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _attendanceRepository.AddAsync(attendance);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.FailedReasons.Add($"Employee {entry.EmployeeId}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<AttendanceResponseDto> GetByIdAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();
        return await LoadResponseAsync(id, subscriptionId);
    }

    public async Task<PagedResultDto<AttendanceResponseDto>> GetFilteredAsync(AttendanceFilterDto filter)
    {
        var subscriptionId = GetSubscriptionId();

        var pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var query = BaseQuery(subscriptionId).AsNoTracking();

        if (filter.EmployeeId is int empId)
        {
            query = query.Where(a => a.EmployeeId == empId);
        }

        if (filter.BranchId is int brId)
        {
            query = query.Where(a => a.Employee.BranchId == brId);
        }

        if (filter.DepartmentId is int deptId)
        {
            query = query.Where(a => a.Employee.DepartmentId == deptId);
        }

        if (filter.FromDate is DateTime from)
        {
            var fromDate = from.Date;
            query = query.Where(a => a.AttendanceDate >= fromDate);
        }

        if (filter.ToDate is DateTime to)
        {
            var toDate = to.Date;
            query = query.Where(a => a.AttendanceDate <= toDate);
        }

        if (filter.Year is int year && filter.Month is int month)
        {
            query = query.Where(a => a.AttendanceDate.Year == year && a.AttendanceDate.Month == month);
        }
        else if (filter.Year is int yearOnly)
        {
            query = query.Where(a => a.AttendanceDate.Year == yearOnly);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(a => a.Status == status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AttendanceDate)
            .ThenBy(a => a.Employee.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<AttendanceResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AttendanceSummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();
        var employee = await ResolveEmployeeForReadAsync(employeeId, subscriptionId);

        var firstOfMonth = new DateTime(year, month, 1);
        var lastOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var records = await _attendanceRepository.Query()
            .AsNoTracking()
            .Where(a =>
                a.EmployeeId == employeeId &&
                a.SubscriptionId == subscriptionId &&
                a.AttendanceDate >= firstOfMonth &&
                a.AttendanceDate <= lastOfMonth)
            .ToListAsync();

        var totalWorkingDays = await _workingDayCalculator
            .CountWorkingDaysAsync(firstOfMonth, lastOfMonth, employee.BranchId);

        var presentDays = records.Count(r => r.Status == "Present" || r.Status == "Late");
        var absentDays = records.Count(r => r.Status == "Absent");
        var halfDays = records.Count(r => r.Status == "HalfDay");
        var lateDays = records.Count(r => r.IsLate);
        var holidayDays = records.Count(r => r.Status == "Holiday");
        var weeklyOffDays = records.Count(r => r.Status == "WeeklyOff");

        var totalLateMinutes = records.Sum(r => r.LateMinutes);
        var totalOvertimeMinutes = records.Sum(r => r.OvertimeMinutes);
        var totalActualWorkingMinutes = records.Sum(r => r.ActualWorkingMinutes);

        var attendancePercentage = totalWorkingDays > 0
            ? Math.Round((decimal)presentDays / totalWorkingDays * 100, 2)
            : 0m;

        return new AttendanceSummaryDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeFullName = employee.FullName,
            Year = year,
            Month = month,
            TotalWorkingDays = totalWorkingDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            HalfDays = halfDays,
            LateDays = lateDays,
            HolidayDays = holidayDays,
            WeeklyOffDays = weeklyOffDays,
            TotalLateMinutes = totalLateMinutes,
            TotalOvertimeMinutes = totalOvertimeMinutes,
            TotalActualWorkingMinutes = totalActualWorkingMinutes,
            AttendancePercentage = attendancePercentage
        };
    }

    public async Task<IEnumerable<AttendanceSummaryDto>> GetMonthlySummaryByBranchAsync(
        int branchId, int year, int month)
    {
        var subscriptionId = GetSubscriptionId();
        await EnsureBranchOwnershipAsync(branchId, subscriptionId);

        var employees = await _employeeRepository.Query()
            .AsNoTracking()
            .Where(e => e.BranchId == branchId && e.SubscriptionId == subscriptionId)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        var summaries = new List<AttendanceSummaryDto>();
        foreach (var employee in employees)
        {
            summaries.Add(await GetMonthlySummaryAsync(employee.Id, year, month));
        }

        return summaries;
    }

    public async Task<AttendanceResponseDto> UpdateAsync(int id, UpdateAttendanceDto dto)
    {
        var subscriptionId = GetSubscriptionId();

        var attendance = await _attendanceRepository.Query()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Attendance with ID {id} not found.");

        if (attendance.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this attendance record.");
        }

        var employee = await ResolveEmployeeAsync(attendance.EmployeeId, subscriptionId);
        var slot = await ResolveDutySlotAsync(dto.DutySlotId, subscriptionId);

        ValidatePunchTimes(dto.PunchInTime, dto.PunchOutTime, slot);

        var (status, isLate, lateMins, actualMins, overtimeMins) =
            await ComputeAttendanceMetricsAsync(attendance.AttendanceDate, dto.PunchInTime, dto.PunchOutTime, slot, employee.BranchId);

        attendance.DutySlotId = slot.Id;
        attendance.PunchInTime = dto.PunchInTime;
        attendance.PunchOutTime = dto.PunchOutTime;
        attendance.Status = status;
        attendance.IsLate = isLate;
        attendance.LateMinutes = lateMins;
        attendance.ActualWorkingMinutes = actualMins;
        attendance.ScheduledWorkingMinutes = (int)(slot.TotalWorkingHours * 60);
        attendance.OvertimeMinutes = overtimeMins;
        attendance.Remarks = dto.Remarks;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _attendanceRepository.UpdateAsync(attendance);

        return await LoadResponseAsync(attendance.Id, subscriptionId);
    }

    public async Task DeleteAsync(int id)
    {
        var subscriptionId = GetSubscriptionId();

        var attendance = await _attendanceRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Attendance with ID {id} not found.");

        if (attendance.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this attendance record.");
        }

        var hasOvertime = await _overtimeRepository.Query()
            .AnyAsync(o => o.AttendanceId == id);

        if (hasOvertime)
        {
            throw new InvalidOperationException("Cannot delete attendance with linked overtime records.");
        }

        await _attendanceRepository.DeleteAsync(attendance);
    }

    private IQueryable<Attendance> BaseQuery(int subscriptionId)
    {
        return _attendanceRepository
            .Query()
            .Include(a => a.Employee)
            .Include(a => a.DutySlot)
            .Where(a => a.SubscriptionId == subscriptionId);
    }

    private async Task<AttendanceResponseDto> LoadResponseAsync(int attendanceId, int subscriptionId)
    {
        var attendance = await BaseQuery(subscriptionId)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attendanceId);

        if (attendance is null)
        {
            var existsForOtherTenant = await _attendanceRepository.Query()
                .AnyAsync(a => a.Id == attendanceId);

            if (existsForOtherTenant)
            {
                throw new UnauthorizedAccessException("Access denied to this attendance record.");
            }

            throw new KeyNotFoundException($"Attendance with ID {attendanceId} not found.");
        }

        return MapToResponseDto(attendance);
    }

    private AttendanceResponseDto MapToResponseDto(Attendance a)
    {
        var dto = _mapper.Map<AttendanceResponseDto>(a);

        dto.AttendanceDateFormatted = a.AttendanceDate.ToString("dddd, dd MMM yyyy");

        dto.ShiftTime = $"{DateTime.Today.Add(a.DutySlot.StartTime):hh:mm tt} – " +
                        $"{DateTime.Today.Add(a.DutySlot.EndTime):hh:mm tt}";

        dto.PunchInFormatted = a.PunchInTime.HasValue
            ? DateTime.Today.Add(a.PunchInTime.Value).ToString("hh:mm tt") : null;

        dto.PunchOutFormatted = a.PunchOutTime.HasValue
            ? DateTime.Today.Add(a.PunchOutTime.Value).ToString("hh:mm tt") : null;

        dto.StatusLabel = a.Status switch
        {
            "Present" => "On Time",
            "Late" => "Late Arrival",
            "Absent" => "Absent",
            "HalfDay" => "Half Day",
            "Holiday" => "Public Holiday",
            "WeeklyOff" => "Weekly Off",
            _ => a.Status
        };

        dto.LateFormatted = a.IsLate ? $"{a.LateMinutes} min late" : string.Empty;

        var ahours = a.ActualWorkingMinutes / 60;
        var amins = a.ActualWorkingMinutes % 60;
        dto.ActualWorkingHoursFormatted = amins > 0 ? $"{ahours}h {amins}m" : $"{ahours}h";

        dto.OvertimeFormatted = a.OvertimeMinutes > 0
            ? $"{a.OvertimeMinutes} min OT" : string.Empty;

        return dto;
    }

    private async Task<(string status, bool isLate, int lateMinutes,
        int actualWorkingMinutes, int overtimeMinutes)>
        ComputeAttendanceMetricsAsync(
            DateTime date, TimeSpan? punchIn, TimeSpan? punchOut,
            DutySlot slot, int? branchId)
    {
        var holidayCheck = await _holidayService.IsHolidayAsync(date, branchId);
        if (holidayCheck.IsHoliday)
        {
            return ("Holiday", false, 0, 0, 0);
        }

        var isOff = await _offDayService.IsOffDayAsync(date, branchId);
        if (isOff)
        {
            return ("WeeklyOff", false, 0, 0, 0);
        }

        if (punchIn is null)
        {
            return ("Absent", false, 0, 0, 0);
        }

        int actualMins = 0;
        if (punchOut.HasValue)
        {
            actualMins = (int)(punchOut.Value - punchIn.Value).TotalMinutes;
            if (actualMins < 0)
            {
                actualMins += 24 * 60;
            }
        }

        int scheduledMins = (int)(slot.TotalWorkingHours * 60);

        var toleranceEnd = slot.StartTime.Add(TimeSpan.FromMinutes(slot.LateToleranceMinutes));
        bool isLate = punchIn.Value > toleranceEnd;
        int lateMins = isLate
            ? (int)(punchIn.Value - toleranceEnd).TotalMinutes
            : 0;

        int overtimeMins = actualMins > scheduledMins
            ? actualMins - scheduledMins
            : 0;

        string status;
        if (punchOut.HasValue && actualMins < scheduledMins / 2)
        {
            status = "HalfDay";
        }
        else
        {
            status = isLate ? "Late" : "Present";
        }

        return (status, isLate, lateMins, actualMins, overtimeMins);
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
                "Attendance can only be recorded for active employees.");
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

    private async Task<DutySlot> ResolveDutySlotAsync(int dutySlotId, int subscriptionId)
    {
        var slot = await _dutySlotRepository.GetByIdAsync(dutySlotId)
            ?? throw new KeyNotFoundException($"DutySlot with ID {dutySlotId} not found.");

        if (slot.SubscriptionId != subscriptionId)
        {
            throw new UnauthorizedAccessException("Access denied to this duty slot.");
        }

        if (!slot.IsActive)
        {
            throw new InvalidOperationException("Cannot record attendance against an inactive duty slot.");
        }

        return slot;
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

    private static void ValidatePunchTimes(TimeSpan? punchIn, TimeSpan? punchOut, DutySlot slot)
    {
        if (punchIn.HasValue && punchOut.HasValue && !slot.IsNightShift)
        {
            if (punchOut.Value <= punchIn.Value)
            {
                throw new InvalidOperationException("PunchOutTime must be after PunchInTime for day shifts.");
            }
        }
    }

    private int GetSubscriptionId()
    {
        return _httpContextAccessor.HttpContext?.User.GetSubscriptionId()
            ?? throw new UnauthorizedAccessException("No authenticated user on the request.");
    }
}

using HRM.Core.DTOs.Attendance;
using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveAllotment;

namespace HRM.API.Services.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceResponseDto> CreateAsync(CreateAttendanceDto dto);
    Task<BulkCreateResultDto> BulkCreateAsync(BulkAttendanceDto dto);
    Task<AttendanceResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<AttendanceResponseDto>> GetFilteredAsync(AttendanceFilterDto filter);
    Task<AttendanceSummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month);
    Task<IEnumerable<AttendanceSummaryDto>> GetMonthlySummaryByBranchAsync(int branchId, int year, int month);
    Task<AttendanceResponseDto> UpdateAsync(int id, UpdateAttendanceDto dto);
    Task DeleteAsync(int id);
}

using HRM.Core.DTOs.LeaveAllotment;

namespace HRM.API.Services.Interfaces;

public interface ILeaveAllotmentService
{
    Task<LeaveAllotmentResponseDto> CreateAsync(CreateLeaveAllotmentDto dto);
    Task<BulkCreateResultDto> BulkCreateAsync(BulkCreateLeaveAllotmentDto dto);
    Task<LeaveAllotmentResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<LeaveAllotmentResponseDto>> GetByEmployeeAsync(int employeeId, int? year = null);
    Task<IEnumerable<LeaveAllotmentResponseDto>> GetByLeaveTypeAsync(int leaveTypeId, int year);
    Task<IEnumerable<LeaveAllotmentResponseDto>> GetByYearAsync(int year);
    Task<LeaveAllotmentResponseDto> UpdateAsync(int id, UpdateLeaveAllotmentDto dto);
    Task DeleteAsync(int id);
}

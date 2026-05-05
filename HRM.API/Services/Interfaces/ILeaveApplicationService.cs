using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.LeaveApplication;

namespace HRM.API.Services.Interfaces;

public interface ILeaveApplicationService
{
    Task<LeaveApplicationResponseDto> CreateAsync(CreateLeaveApplicationDto dto);
    Task<LeaveApplicationResponseDto> UploadAttachmentAsync(int id, IFormFile file);
    Task<LeaveApplicationResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<LeaveApplicationResponseDto>> GetFilteredAsync(LeaveApplicationFilterDto filter);
    Task<IEnumerable<LeaveApplicationResponseDto>> GetByEmployeeAsync(int employeeId, int? year = null);
    Task<LeaveApplicationResponseDto> ApproveAsync(int id, ApproveLeaveDto dto);
    Task<LeaveApplicationResponseDto> RejectAsync(int id, RejectLeaveDto dto);
    Task<LeaveApplicationResponseDto> CancelAsync(int id, CancelLeaveDto dto);
    Task DeleteAsync(int id);
}

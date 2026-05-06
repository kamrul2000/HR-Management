using HRM.Core.DTOs.Employee;
using HRM.Core.DTOs.EmployeeSeparation;

namespace HRM.API.Services.Interfaces;

public interface IEmployeeSeparationService
{
    Task<SeparationResponseDto> CreateAsync(CreateSeparationDto dto);
    Task<SeparationResponseDto> UploadAttachmentAsync(int id, IFormFile file);
    Task<SeparationResponseDto> GetByIdAsync(int id);
    Task<SeparationResponseDto> GetByEmployeeAsync(int employeeId);
    Task<PagedResultDto<SeparationResponseDto>> GetFilteredAsync(SeparationFilterDto filter);
    Task<SeparationResponseDto> ApproveAsync(int id, ApproveSeparationDto dto);
    Task<SeparationResponseDto> ProcessAsync(int id);
    Task<SeparationResponseDto> CancelAsync(int id, CancelSeparationDto dto);
}

using HRM.Core.DTOs.Employee;

namespace HRM.API.Services.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto);
    Task<EmployeeResponseDto> GetByIdAsync(int id);
    Task<PagedResultDto<EmployeeListDto>> GetFilteredAsync(EmployeeFilterDto filter);
    Task<IEnumerable<EmployeeListDto>> GetByBranchAsync(int branchId);
    Task<IEnumerable<EmployeeListDto>> GetByDepartmentAsync(int departmentId);
    Task<EmployeeResponseDto> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<EmployeeResponseDto> UploadPhotoAsync(int id, IFormFile photo);
    Task DeleteAsync(int id);
}

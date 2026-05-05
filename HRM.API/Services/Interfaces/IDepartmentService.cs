using HRM.Core.DTOs.Department;

namespace HRM.API.Services.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<DepartmentResponseDto>> GetAllAsync();
    Task<IEnumerable<DepartmentResponseDto>> GetByBranchAsync(int branchId);
    Task<DepartmentResponseDto> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task DeleteAsync(int id);
}

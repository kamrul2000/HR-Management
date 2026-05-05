using HRM.Core.DTOs.Designation;

namespace HRM.API.Services.Interfaces;

public interface IDesignationService
{
    Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto);
    Task<DesignationResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<DesignationResponseDto>> GetAllAsync();
    Task<IEnumerable<DesignationResponseDto>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<DesignationResponseDto>> GetByBranchAsync(int branchId);
    Task<DesignationResponseDto> UpdateAsync(int id, UpdateDesignationDto dto);
    Task DeleteAsync(int id);
}

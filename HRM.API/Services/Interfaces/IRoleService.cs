using HRM.Core.DTOs.Role;

namespace HRM.API.Services.Interfaces;

public interface IRoleService
{
    Task<RoleResponseDto> CreateAsync(CreateRoleDto dto);
    Task<RoleResponseDto> GetByIdAsync(int id);
    Task<IEnumerable<RoleResponseDto>> GetAllAsync();
    Task<IEnumerable<RoleResponseDto>> GetActiveAsync();
    Task<RoleResponseDto> UpdateAsync(int id, UpdateRoleDto dto);
    Task DeleteAsync(int id);
}

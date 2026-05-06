using HRM.Core.DTOs.UserRole;

namespace HRM.API.Services.Interfaces;

public interface IUserRoleService
{
    Task<UserRoleResponseDto> AssignAsync(AssignRoleDto dto);
    Task<UserRoleResponseDto> RevokeAsync(int userRoleId);
    Task<IEnumerable<UserRoleResponseDto>> GetByUserAsync(int userId);
    Task<IEnumerable<UserRoleResponseDto>> GetByRoleAsync(int roleId);
    Task<IEnumerable<UserRoleResponseDto>> GetAllActiveAsync();
}

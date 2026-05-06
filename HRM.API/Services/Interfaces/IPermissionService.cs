using HRM.Core.DTOs.Permission;

namespace HRM.API.Services.Interfaces;

public interface IPermissionService
{
    Task<PermissionResponseDto> UpsertAsync(UpsertPermissionDto dto);
    Task<IEnumerable<PermissionResponseDto>> BulkUpsertAsync(BulkUpsertPermissionsDto dto);
    Task<IEnumerable<PermissionResponseDto>> GetByRoleAsync(int roleId);
    Task<UserPermissionSummaryDto> GetMyPermissionsAsync();
    Task<IEnumerable<PermissionResponseDto>> GetAllAsync();
    Task DeleteAsync(int id);
}

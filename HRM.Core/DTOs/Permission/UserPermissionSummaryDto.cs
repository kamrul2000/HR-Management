namespace HRM.Core.DTOs.Permission;

public class UserPermissionSummaryDto
{
    public int UserId { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<PermissionResponseDto> Permissions { get; set; } = new();
}

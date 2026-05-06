namespace HRM.Core.DTOs.UserRole;

public class UserRoleResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int AssignedById { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedAtFormatted { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedAtFormatted { get; set; }
    public int? RevokedById { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

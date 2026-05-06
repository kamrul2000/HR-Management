using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class UserRole
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    public DateTime AssignedAt { get; set; }

    [Required]
    public int AssignedById { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? RevokedAt { get; set; }

    public int? RevokedById { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

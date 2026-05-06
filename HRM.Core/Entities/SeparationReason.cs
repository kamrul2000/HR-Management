using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class SeparationReason
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string ReasonName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string SeparationType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 999)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

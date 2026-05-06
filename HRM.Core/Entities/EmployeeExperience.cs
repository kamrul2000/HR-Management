using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class EmployeeExperience
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(200)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Designation { get; set; } = string.Empty;

    [Required]
    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool IsCurrent { get; set; }

    [MaxLength(1000)]
    public string? Responsibilities { get; set; }

    [MaxLength(500)]
    public string? ReasonForLeaving { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

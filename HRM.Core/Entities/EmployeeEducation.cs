using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class EmployeeEducation
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Degree { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Institution { get; set; } = string.Empty;

    [Required]
    [Range(1950, 2100)]
    public int PassingYear { get; set; }

    [MaxLength(50)]
    public string? Result { get; set; }

    [MaxLength(100)]
    public string? MajorSubject { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class TaxExclusion
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ExclusionType { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? PartialExclusionAmount { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [MaxLength(100)]
    public string? CertificateNo { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

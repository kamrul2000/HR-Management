using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.TaxExclusion;

public class CreateTaxExclusionDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

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
}

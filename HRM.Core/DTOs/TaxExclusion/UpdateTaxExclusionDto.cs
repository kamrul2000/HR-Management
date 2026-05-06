using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.TaxExclusion;

public class UpdateTaxExclusionDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? PartialExclusionAmount { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [MaxLength(100)]
    public string? CertificateNo { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

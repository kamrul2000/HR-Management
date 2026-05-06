using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.TaxSlab;

public class CreateTaxSlabConfigDto
{
    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal TaxFreeThreshold { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MinLength(1)]
    public List<TaxSlabDto> Slabs { get; set; } = new();
}

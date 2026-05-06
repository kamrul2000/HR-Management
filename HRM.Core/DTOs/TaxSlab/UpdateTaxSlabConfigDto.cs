using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.TaxSlab;

public class UpdateTaxSlabConfigDto
{
    [Required]
    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal TaxFreeThreshold { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [Required]
    [MinLength(1)]
    public List<TaxSlabDto> Slabs { get; set; } = new();
}

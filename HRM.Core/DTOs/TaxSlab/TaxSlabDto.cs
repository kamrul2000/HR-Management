using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.TaxSlab;

public class TaxSlabDto
{
    [Required]
    [Range(1, 20)]
    public int SlabOrder { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal MinAmount { get; set; }

    /// <summary>Null for the top slab (no upper limit).</summary>
    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? MaxAmount { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRate { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class TaxSlab
{
    public int Id { get; set; }

    [Required]
    public int TaxSlabConfigId { get; set; }

    public TaxSlabConfig TaxSlabConfig { get; set; } = null!;

    [Required]
    [Range(1, 20)]
    public int SlabOrder { get; set; }

    [Required]
    public decimal MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRate { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class TaxSlabConfig
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public decimal TaxFreeThreshold { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<TaxSlab> Slabs { get; set; } = new List<TaxSlab>();
}

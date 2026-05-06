namespace HRM.Core.DTOs.TaxSlab;

public class TaxSlabConfigResponseDto
{
    public int Id { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string StartDateFormatted { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public string EndDateFormatted { get; set; } = string.Empty;
    public decimal TaxFreeThreshold { get; set; }
    public string TaxFreeThresholdFormatted { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<TaxSlabResponseDto> Slabs { get; set; } = new();
    public int SlabCount { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.TaxSlab;

public class TaxSlabResponseDto
{
    public int Id { get; set; }
    public int SlabOrder { get; set; }
    public decimal MinAmount { get; set; }
    public string MinAmountFormatted { get; set; } = string.Empty;
    public decimal? MaxAmount { get; set; }
    public string MaxAmountFormatted { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string TaxRateLabel { get; set; } = string.Empty;
    public string RangeLabel { get; set; } = string.Empty;
}

namespace HRM.Core.DTOs.TaxSlab;

public class TaxComputationResultDto
{
    public decimal AnnualIncome { get; set; }
    public string AnnualIncomeFormatted { get; set; } = string.Empty;
    public decimal TaxFreeThreshold { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal AnnualTax { get; set; }
    public string AnnualTaxFormatted { get; set; } = string.Empty;
    public decimal MonthlyTax { get; set; }
    public string MonthlyTaxFormatted { get; set; } = string.Empty;
    public decimal EffectiveTaxRate { get; set; }
    public string EffectiveTaxRateLabel { get; set; } = string.Empty;
    public string FiscalYear { get; set; } = string.Empty;
    public List<TaxSlabBreakdownDto> SlabBreakdown { get; set; } = new();
}

public class TaxSlabBreakdownDto
{
    public int SlabOrder { get; set; }
    public string RangeLabel { get; set; } = string.Empty;
    public decimal AmountInBand { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string TaxAmountFormatted { get; set; } = string.Empty;
}

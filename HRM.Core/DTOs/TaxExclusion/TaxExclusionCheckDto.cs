namespace HRM.Core.DTOs.TaxExclusion;

public class TaxExclusionCheckDto
{
    public bool IsExcluded { get; set; }

    /// <summary>"Full", "Partial", or "" when not excluded.</summary>
    public string ExclusionType { get; set; } = string.Empty;

    public decimal? PartialExclusionAmount { get; set; }
}

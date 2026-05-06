using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Bonus;

public class ApproveBonusDto
{
    /// <summary>
    /// Approver's final bonus amount. If null, defaults to ComputedAmount.
    /// </summary>
    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? FinalAmount { get; set; }

    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }
}

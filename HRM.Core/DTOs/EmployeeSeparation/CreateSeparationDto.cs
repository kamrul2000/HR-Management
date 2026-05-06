using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.EmployeeSeparation;

public class CreateSeparationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SeparationReasonId { get; set; }

    [Required]
    [MaxLength(30)]
    public string SeparationType { get; set; } = string.Empty;

    [Required]
    public DateTime ApplicationDate { get; set; }

    [Required]
    public DateTime LastWorkingDate { get; set; }

    [Required]
    [Range(0, 365)]
    public int NoticePeriodDays { get; set; }

    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal NoticePeriodBuyout { get; set; }

    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal OtherSettlementAmount { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}

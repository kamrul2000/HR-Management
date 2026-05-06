using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.PfInterest;

public class ComputePfInterestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SalaryCalculation;

public class RunSalaryCalculationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

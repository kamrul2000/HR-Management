using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SalaryCalculation;

public class BulkRunSalaryDto
{
    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    /// <summary>
    /// Optional: limit to specific employees.
    /// If empty, calculates for all active employees in the tenant (or branch).
    /// </summary>
    public List<int> EmployeeIds { get; set; } = new();

    /// <summary>Optional: limit to a specific branch.</summary>
    public int? BranchId { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

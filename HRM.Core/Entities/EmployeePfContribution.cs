using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class EmployeePfContribution
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int PfRuleId { get; set; }

    public PfRule PfRule { get; set; } = null!;

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    public decimal PfBase { get; set; }

    [Required]
    public decimal EmployeeContributionRate { get; set; }

    [Required]
    public decimal EmployerContributionRate { get; set; }

    [Required]
    public decimal EmployeeContribution { get; set; }

    [Required]
    public decimal EmployerContribution { get; set; }

    [Required]
    public decimal TotalContribution { get; set; }

    public int? SalaryCalculationId { get; set; }

    public SalaryCalculation? SalaryCalculation { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

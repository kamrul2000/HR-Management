using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.Core.Entities;

public class LeaveAllotment
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int LeaveTypeId { get; set; }

    public LeaveType LeaveType { get; set; } = null!;

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    public decimal AllocatedDays { get; set; }

    public decimal UsedDays { get; set; }

    public decimal CarriedForwardDays { get; set; }

    [NotMapped]
    public decimal RemainingDays => AllocatedDays + CarriedForwardDays - UsedDays;

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

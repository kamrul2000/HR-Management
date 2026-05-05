namespace HRM.Core.Entities;

// Stub introduced in Module 6 so EmployeeService.DeleteAsync can guard against
// hard-deleting employees with leave applications. Module 13 extends this with
// full leave-application fields.
public class LeaveApplication
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.Entities;

// Stub introduced in Module 6 so EmployeeService.DeleteAsync can guard against
// hard-deleting employees with salary structures. Module 16 extends this with
// full salary-assignment fields.
public class SalaryCreate
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

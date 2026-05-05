namespace HRM.Core.Entities;

// Stub introduced in Module 6 so EmployeeService.DeleteAsync can guard against
// hard-deleting employees with attendance records. Module 12 extends this with
// full attendance-tracking fields.
public class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.Entities;

// Stub introduced in Module 12 so AttendanceService.DeleteAsync can guard
// against deleting attendance rows that are referenced by overtime records.
// Module 14 extends this with full overtime fields.
public class Overtime
{
    public int Id { get; set; }

    public int AttendanceId { get; set; }

    public Attendance Attendance { get; set; } = null!;

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

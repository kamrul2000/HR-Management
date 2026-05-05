namespace HRM.Core.Entities;

// Stub introduced in Module 6 so EmployeeService.DeleteAsync can guard against
// hard-deleting employees with attendance records. Module 7 added DutySlotId so
// DutySlotService.DeleteAsync can guard against deleting slots referenced by
// attendance. Module 12 extends this with the full attendance-tracking fields.
public class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int? DutySlotId { get; set; }

    public DutySlot? DutySlot { get; set; }

    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

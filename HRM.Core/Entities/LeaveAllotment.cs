namespace HRM.Core.Entities;

// Stub introduced in Module 8 so LeaveType.LeaveAllotments navigation compiles
// and LeaveTypeService.DeleteAsync can guard against deleting types with active
// allotments. Module 9 extends this with full allotment fields (EmployeeId,
// AllottedDays, UsedDays, Year, etc.).
public class LeaveAllotment
{
    public int Id { get; set; }

    public int LeaveTypeId { get; set; }

    public LeaveType LeaveType { get; set; } = null!;

    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

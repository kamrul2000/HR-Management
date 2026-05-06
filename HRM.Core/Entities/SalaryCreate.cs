namespace HRM.Core.Entities;

// Stub introduced in Module 6 so EmployeeService.DeleteAsync can guard against
// hard-deleting employees with salary structures. Module 15 added SalaryHeadId
// (nullable) so SalaryHeadService.DeleteAsync can guard against deleting heads
// referenced by salary structures. Module 16 extends this with full salary-
// structure fields and is expected to make SalaryHeadId required.
public class SalaryCreate
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public int? SalaryHeadId { get; set; }

    public SalaryHead? SalaryHead { get; set; }

    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

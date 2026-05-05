namespace HRM.Core.DTOs.LeaveAllotment;

public class LeaveAllotmentResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public int Year { get; set; }
    public decimal AllocatedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
    public decimal RemainingDays { get; set; }
    public int SubscriptionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.LeaveType;

public class LeaveTypeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public string IsPaidLabel { get; set; } = string.Empty;
    public bool IsCarryForward { get; set; }
    public int MaxCarryForwardDays { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresDocument { get; set; }
    public int MinNoticeDays { get; set; }
    public int? MaxConsecutiveDays { get; set; }
    public string? GenderRestriction { get; set; }
    public string GenderRestrictionLabel { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.AdditionalInfo;

public class ExperienceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public string FromDateFormatted { get; set; } = string.Empty;
    public DateTime? ToDate { get; set; }
    public string? ToDateFormatted { get; set; }
    public bool IsCurrent { get; set; }
    public string? Responsibilities { get; set; }
    public string? ReasonForLeaving { get; set; }
    public string? AttachmentPath { get; set; }
    public string? AttachmentUrl { get; set; }
    public string ServiceDurationLabel { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.AdditionalInfo;

public class EducationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int PassingYear { get; set; }
    public string? Result { get; set; }
    public string? MajorSubject { get; set; }
    public string? AttachmentPath { get; set; }
    public string? AttachmentUrl { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

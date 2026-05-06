namespace HRM.Core.DTOs.AdditionalInfo;

public class EmergencyContactDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? Address { get; set; }
    public bool IsPrimary { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

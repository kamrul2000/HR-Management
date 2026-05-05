namespace HRM.Core.DTOs.HolidayCalendar;

public class HolidayResponseDto
{
    public int Id { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string HolidayDateFormatted { get; set; } = string.Empty;
    public string HolidayType { get; set; } = string.Empty;
    public string HolidayTypeLabel { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurringYearly { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsOrganizationWide { get; set; }
    public int SubscriptionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.HolidayCalendar;

public class HolidayFilterDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? BranchId { get; set; }
    public string? HolidayType { get; set; }
    public bool IncludeInactive { get; set; }
}

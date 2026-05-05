namespace HRM.Core.DTOs.OffDay;

public class OffDayScheduleDto
{
    /// <summary>Branch this schedule applies to. Null = organization-wide.</summary>
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }

    /// <summary>Source of the schedule: "Branch" or "Organization" (fallback).</summary>
    public string ConfigurationSource { get; set; } = string.Empty;

    /// <summary>List of DayOfWeek integers that are off.</summary>
    public List<int> OffDays { get; set; } = new();

    /// <summary>Human-readable day names.</summary>
    public List<string> OffDayNames { get; set; } = new();

    /// <summary>Total off days per week.</summary>
    public int TotalOffDaysPerWeek { get; set; }
}

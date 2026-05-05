using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.OffDay;

public class BulkSetOffDaysDto
{
    /// <summary>
    /// List of DayOfWeek values (0=Sunday … 6=Saturday) to mark as off days.
    /// All existing off days for this scope are replaced. Pass an empty list to clear.
    /// </summary>
    [Required]
    public List<int> DaysOfWeek { get; set; } = new();

    /// <summary>Target branch. Null = organization-wide configuration.</summary>
    public int? BranchId { get; set; }
}

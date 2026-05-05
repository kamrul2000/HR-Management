using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.HolidayCalendar;

public class CreateHolidayDto
{
    [Required]
    [MaxLength(150)]
    public string HolidayName { get; set; } = string.Empty;

    [Required]
    public DateTime HolidayDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string HolidayType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsRecurringYearly { get; set; }

    public int? BranchId { get; set; }
}

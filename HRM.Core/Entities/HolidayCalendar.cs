using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class HolidayCalendar
{
    public int Id { get; set; }

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

    public bool IsRecurringYearly { get; set; }

    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

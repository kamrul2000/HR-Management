using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.OffDay;

public class CreateOffDayDto
{
    [Required]
    [Range(0, 6)]
    public int DayOfWeek { get; set; }

    public int? BranchId { get; set; }
}

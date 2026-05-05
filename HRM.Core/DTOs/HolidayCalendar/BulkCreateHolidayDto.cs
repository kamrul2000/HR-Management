using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.HolidayCalendar;

public class BulkCreateHolidayDto
{
    [Required]
    [MinLength(1)]
    public List<CreateHolidayDto> Holidays { get; set; } = new();
}

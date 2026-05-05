using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.OffDay;

public class UpdateOffDayDto
{
    [Required]
    public bool IsActive { get; set; }
}

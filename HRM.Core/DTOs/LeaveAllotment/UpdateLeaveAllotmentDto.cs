using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveAllotment;

public class UpdateLeaveAllotmentDto
{
    [Required]
    [Range(typeof(decimal), "0.5", "365")]
    public decimal AllocatedDays { get; set; }

    [Range(typeof(decimal), "0", "365")]
    public decimal CarriedForwardDays { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.PfInterest;

public class BulkComputePfInterestDto
{
    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;

    public int? BranchId { get; set; }
}

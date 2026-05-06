namespace HRM.Core.DTOs.Bonus;

public class BonusFilterDto
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public string? BonusType { get; set; }
    public string? Status { get; set; }
    public int? DisbursementMonth { get; set; }
    public int? DisbursementYear { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

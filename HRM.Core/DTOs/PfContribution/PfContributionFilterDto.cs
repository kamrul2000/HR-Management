namespace HRM.Core.DTOs.PfContribution;

public class PfContributionFilterDto
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

namespace HRM.Core.DTOs.LoanApplication;

public class LoanApplicationFilterDto
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public string? LoanType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

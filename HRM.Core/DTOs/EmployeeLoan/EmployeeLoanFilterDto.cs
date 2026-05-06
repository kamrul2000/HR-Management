namespace HRM.Core.DTOs.EmployeeLoan;

public class EmployeeLoanFilterDto
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; }
    public string? LoanType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

namespace HRM.Core.DTOs.LoanInstallment;

public class InstallmentFilterDto
{
    public int? EmployeeId { get; set; }
    public int? EmployeeLoanId { get; set; }
    public string? Status { get; set; }
    public int? DueMonth { get; set; }
    public int? DueYear { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

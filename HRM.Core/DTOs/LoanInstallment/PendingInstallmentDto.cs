namespace HRM.Core.DTOs.LoanInstallment;

public class PendingInstallmentDto
{
    public int InstallmentId { get; set; }
    public int EmployeeLoanId { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int InstallmentNo { get; set; }
    public int DueMonth { get; set; }
    public int DueYear { get; set; }
}

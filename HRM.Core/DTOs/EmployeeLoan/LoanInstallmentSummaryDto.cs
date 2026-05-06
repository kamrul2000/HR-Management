namespace HRM.Core.DTOs.EmployeeLoan;

public class LoanInstallmentSummaryDto
{
    public int Id { get; set; }
    public int InstallmentNo { get; set; }
    public int DueMonth { get; set; }
    public int DueYear { get; set; }
    public string DuePeriodLabel { get; set; } = string.Empty;
    public decimal InstallmentAmount { get; set; }
    public string InstallmentAmountFormatted { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
}

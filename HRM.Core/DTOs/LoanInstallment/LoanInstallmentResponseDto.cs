namespace HRM.Core.DTOs.LoanInstallment;

public class LoanInstallmentResponseDto
{
    public int Id { get; set; }
    public int EmployeeLoanId { get; set; }
    public string LoanNo { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int InstallmentNo { get; set; }
    public int DueMonth { get; set; }
    public int DueYear { get; set; }
    public string DuePeriodLabel { get; set; } = string.Empty;
    public decimal InstallmentAmount { get; set; }
    public string InstallmentAmountFormatted { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public string PaidAmountFormatted { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaidDateFormatted { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int? SalaryCalculationId { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

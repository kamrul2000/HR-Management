namespace HRM.Core.DTOs.SalaryCalculation;

public class SalaryCalculationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int SalaryStructureId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal LateDeductionDays { get; set; }
    public int OvertimeMinutes { get; set; }
    public string OvertimeFormatted { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public string BasicSalaryFormatted { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public string GrossSalaryFormatted { get; set; } = string.Empty;
    public decimal AttendanceDeduction { get; set; }
    public string AttendanceDeductionFormatted { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public string TotalEarningsFormatted { get; set; } = string.Empty;
    public decimal OvertimePay { get; set; }
    public string OvertimePayFormatted { get; set; } = string.Empty;
    public decimal BonusAmount { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal TotalDeductions { get; set; }
    public string TotalDeductionsFormatted { get; set; } = string.Empty;
    public decimal NetSalary { get; set; }
    public string NetSalaryFormatted { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime? FinalizedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Remarks { get; set; }
    public List<SalaryCalculationDetailResponseDto> EarningDetails { get; set; } = new();
    public List<SalaryCalculationDetailResponseDto> DeductionDetails { get; set; } = new();
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

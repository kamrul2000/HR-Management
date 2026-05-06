namespace HRM.Core.DTOs.SalaryCalculation;

public class SalaryCalculationFilterDto
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

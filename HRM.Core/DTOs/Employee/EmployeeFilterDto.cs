namespace HRM.Core.DTOs.Employee;

public class EmployeeFilterDto
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public string? EmploymentType { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

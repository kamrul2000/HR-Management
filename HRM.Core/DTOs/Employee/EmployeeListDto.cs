namespace HRM.Core.DTOs.Employee;

public class EmployeeListDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationTitle { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; }
}

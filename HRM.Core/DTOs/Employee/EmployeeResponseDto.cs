namespace HRM.Core.DTOs.Employee;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public DateTime JoiningDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int DesignationId { get; set; }
    public string DesignationTitle { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

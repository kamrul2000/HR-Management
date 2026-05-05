using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Employee;

public class CreateEmployeeDto
{
    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MaritalStatus { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NationalId { get; set; }

    [Required]
    public DateTime JoiningDate { get; set; }

    public DateTime? ConfirmationDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int BranchId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DesignationId { get; set; }

    [Required]
    [MaxLength(30)]
    public string EmploymentType { get; set; } = string.Empty;
}

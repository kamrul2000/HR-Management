using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required]
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

    [MaxLength(500)]
    public string? PhotoPath { get; set; }

    [Required]
    public int BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    [Required]
    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    [Required]
    public int DesignationId { get; set; }

    public Designation Designation { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string EmploymentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

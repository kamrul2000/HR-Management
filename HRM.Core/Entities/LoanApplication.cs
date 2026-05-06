using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class LoanApplication
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string ApplicationNo { get; set; } = string.Empty;

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string LoanType { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "1", "9999999.99")]
    public decimal RequestedAmount { get; set; }

    [Required]
    [Range(1, 120)]
    public int RequestedTenureMonths { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Purpose { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    public int? RecommendedById { get; set; }

    public DateTime? RecommendationDate { get; set; }

    [MaxLength(500)]
    public string? RecommendationRemarks { get; set; }

    public int? RejectedById { get; set; }

    public DateTime? RejectionDate { get; set; }

    [MaxLength(500)]
    public string? RejectionRemarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

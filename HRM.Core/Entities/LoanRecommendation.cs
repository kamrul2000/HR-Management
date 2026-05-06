using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class LoanRecommendation
{
    public int Id { get; set; }

    [Required]
    public int LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    [Required]
    public int RecommendedById { get; set; }

    [Required]
    [MaxLength(20)]
    public string Decision { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "9999999.99")]
    public decimal? RecommendedAmount { get; set; }

    [Range(1, 120)]
    public int? RecommendedTenureMonths { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Remarks { get; set; } = string.Empty;

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LoanRecommendation;

public class CreateRecommendationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int LoanApplicationId { get; set; }

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
}

namespace HRM.Core.DTOs.SeparationReason;

public class SeparationReasonResponseDto
{
    public int Id { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string SeparationType { get; set; } = string.Empty;
    public string SeparationTypeLabel { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

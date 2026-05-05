using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int SubscriptionId { get; set; }
}

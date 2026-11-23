using System.ComponentModel.DataAnnotations;

namespace CanadaImmigration.Api.Models;

public class SubscriptionRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = new();
}

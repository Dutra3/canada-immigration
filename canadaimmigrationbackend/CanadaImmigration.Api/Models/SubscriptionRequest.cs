namespace CanadaImmigration.Api.Models;

public class SubscriptionRequest
{
    public string Email { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = new();
}

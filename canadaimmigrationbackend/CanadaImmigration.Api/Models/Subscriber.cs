namespace CanadaImmigration.Api.Models;

public class Subscriber
{
    public const string AllCategoriesValue = "ALL";

    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<SubscriberCategory> Categories { get; set; } = new();
}

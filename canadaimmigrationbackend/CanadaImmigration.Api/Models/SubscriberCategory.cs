namespace CanadaImmigration.Api.Models;

public class SubscriberCategory
{
    public int Id { get; set; }

    public int SubscriberId { get; set; }
    public Subscriber Subscriber { get; set; } = null!;

    public string Category { get; set; } = string.Empty;
}

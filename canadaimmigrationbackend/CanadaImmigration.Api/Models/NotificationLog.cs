namespace CanadaImmigration.Api.Models;

public class NotificationLog
{
    public int Id { get; set; }

    public int? SubscriberId { get; set; }

    public string Email { get; set; } = string.Empty;

    public int DrawNumber { get; set; }

    public string? Category { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}

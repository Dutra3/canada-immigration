namespace CanadaImmigration.Api.Models;

public class DrawCheckState
{
    public int Id { get; set; }

    public int LastDrawNumber { get; set; }

    public DateTime LastCheckedAtUtc { get; set; }
}

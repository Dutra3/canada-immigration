namespace CanadaImmigration.Api.Models;

// Guarda qual foi o último draw processado pelo poller, para saber quando surge um novo.
// Sempre terá no máximo 1 linha (Id = 1).
public class DrawCheckState
{
    public int Id { get; set; }

    public int LastDrawNumber { get; set; }

    public DateTime LastCheckedAtUtc { get; set; }
}

using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

public class ExpressEntryDraw
{
    [JsonPropertyName("drawNumber")]
    public int DrawNumber { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("invitationsIssued")]
    public int InvitationsIssued { get; set; }

    [JsonPropertyName("minimumCRS")]
    public int MinimumCrs { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public string Year { get; set; } = string.Empty;
}

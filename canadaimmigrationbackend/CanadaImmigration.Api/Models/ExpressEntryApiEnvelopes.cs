using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

internal class DrawsApiEnvelope
{
    [JsonPropertyName("draws")]
    public List<ExpressEntryDraw> Draws { get; set; } = new();
}

internal class SingleDrawApiEnvelope
{
    [JsonPropertyName("draw")]
    public ExpressEntryDraw? Draw { get; set; }
}

internal class PoolApiEnvelope
{
    [JsonPropertyName("pool")]
    public PoolDistribution? Pool { get; set; }
}

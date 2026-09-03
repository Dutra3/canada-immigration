using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

// A API externa embrulha os resultados em { "draws": [...] } e { "draw": {...} }.
// Esses envelopes existem só pra desserializar; o Controller nunca expõe eles direto.
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

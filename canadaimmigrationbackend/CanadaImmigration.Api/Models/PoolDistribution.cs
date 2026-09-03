using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

public class PoolDistribution
{
    [JsonPropertyName("drawNumber")]
    public int DrawNumber { get; set; }

    [JsonPropertyName("drawDate")]
    public DateTime DrawDate { get; set; }

    [JsonPropertyName("ranges")]
    public List<PoolScoreRange> Ranges { get; set; } = new();
}

public class PoolScoreRange
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("range")]
    public string Range { get; set; } = string.Empty;

    // Vem como string com vírgula de milhar (ex: "19,542"), por isso não é int.
    // A linha com Range="Total" também usa esse mesmo campo pro total geral.
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

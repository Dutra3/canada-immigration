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

    [JsonPropertyName("totalCandidates")]
    public decimal TotalCandidates { get; set; }
}

public class PoolScoreRange
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("range")]
    public string Range { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    [JsonConverter(typeof(CommaSeparatedDecimalConverter))]
    public decimal Value { get; set; }
}
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

public class CommaSeparatedDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal();
        }

        var raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException("Valor do pool veio vazio/nulo onde um número era esperado.");
        }

        if (!decimal.TryParse(
                raw,
                NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var value))
        {
            throw new JsonException($"Não foi possível converter '{raw}' para decimal (formato inesperado da API externa).");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
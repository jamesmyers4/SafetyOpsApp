using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SafetyOps.Api.Features.Common;

/// <summary>The API's current wire format for dates is US-style <c>MM/dd/yyyy</c>.</summary>
public static class DateFormats
{
    public const string Wire = "MM/dd/yyyy";
    private static readonly string[] Accepted = [Wire, "M/d/yyyy", "yyyy-MM-dd"];

    public static string ToWire(DateOnly date) => date.ToString(Wire, CultureInfo.InvariantCulture);

    public static bool TryParse(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value?.Trim(), Accepted, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}

/// <summary>Serializes <see cref="DateOnly"/> in <see cref="DateFormats.Wire"/> format and accepts the other supported formats.</summary>
public sealed class WireDateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateFormats.TryParse(reader.GetString(), out var date)
            ? date
            : throw new JsonException($"Dates must be in {DateFormats.Wire} format.");

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(DateFormats.ToWire(value));
}

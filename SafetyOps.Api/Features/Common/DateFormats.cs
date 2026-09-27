using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SafetyOps.Api.Features.Common;

/// <summary>
/// Parses dates typed into search boxes. Request and response bodies use ISO 8601
/// (<c>yyyy-MM-dd</c>); people searching tend to type US-style dates.
/// </summary>
public static class DateFormats
{
    private static readonly string[] Accepted = ["MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd"];

    public static bool TryParse(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value?.Trim(), Accepted, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}

/// <summary>Reads and writes <see cref="DateOnly"/> as ISO 8601 (<c>yyyy-MM-dd</c>) with a readable error for anything else.</summary>
public sealed class IsoDateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateOnly.TryParseExact(reader.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new JsonException("Dates must be in yyyy-MM-dd format.");

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}

using System.Globalization;

namespace SafetyOps.Api.Data;

/// <summary>The API's current wire format for dates is US-style <c>MM/dd/yyyy</c>.</summary>
public static class DateFormats
{
    public const string Wire = "MM/dd/yyyy";
    private static readonly string[] Accepted = [Wire, "M/d/yyyy", "yyyy-MM-dd"];

    public static string ToWire(DateOnly date) => date.ToString(Wire, CultureInfo.InvariantCulture);

    public static bool TryParse(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value?.Trim(), Accepted, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}

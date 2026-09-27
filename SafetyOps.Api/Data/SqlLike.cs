namespace SafetyOps.Api.Data;

/// <summary>Builds escaped LIKE patterns. SQLite's LIKE is case-insensitive for ASCII, matching the old in-memory search.</summary>
public static class SqlLike
{
    public const string Escape = @"\";

    public static string Contains(string term) =>
        "%" + term.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_") + "%";
}

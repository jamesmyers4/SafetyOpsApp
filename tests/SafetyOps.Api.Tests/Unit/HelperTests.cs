using SafetyOps.Api.Data;
using SafetyOps.Api.Features.Common;

namespace SafetyOps.Api.Tests.Unit;

public class SqlLikeTests
{
    [TestCase("smith", "%smith%")]
    [TestCase("  padded  ", "%padded%")]
    [TestCase("100%", @"%100\%%")]
    [TestCase("a_b", @"%a\_b%")]
    [TestCase(@"c:\temp", @"%c:\\temp%")]
    public void Contains_wraps_and_escapes_the_term(string term, string expected) =>
        Assert.That(SqlLike.Contains(term), Is.EqualTo(expected));
}

public class DateFormatsTests
{
    [TestCase("06/14/2026")]
    [TestCase("6/14/2026")]
    [TestCase("2026-06-14")]
    [TestCase(" 06/14/2026 ")]
    public void TryParse_accepts_us_and_iso_dates(string value)
    {
        Assert.That(DateFormats.TryParse(value, out var date), Is.True);
        Assert.That(date, Is.EqualTo(new DateOnly(2026, 6, 14)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("14/06/2026")]
    [TestCase("June 14")]
    [TestCase("42")]
    public void TryParse_rejects_anything_else(string? value) =>
        Assert.That(DateFormats.TryParse(value, out _), Is.False);
}

public class PagedResultTests
{
    [TestCase(0, 25, 0)]
    [TestCase(1, 25, 1)]
    [TestCase(25, 25, 1)]
    [TestCase(26, 25, 2)]
    [TestCase(101, 10, 11)]
    public void TotalPages_rounds_up(int totalCount, int pageSize, int expected) =>
        Assert.That(new PagedResult<int>([], 1, pageSize, totalCount).TotalPages, Is.EqualTo(expected));
}

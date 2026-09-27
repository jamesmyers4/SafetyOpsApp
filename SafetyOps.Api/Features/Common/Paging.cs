using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SafetyOps.Api.Features.Common;

/// <summary>Query-string parameters shared by list endpoints.</summary>
public sealed record ListQuery
{
    public const int MaxPageSize = 100;

    /// <summary>Optional free-text filter.</summary>
    [StringLength(100)]
    public string? Search { get; init; }

    /// <summary>1-based page number.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    /// <summary>Items per page (1–100).</summary>
    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = 25;
}

/// <summary>One page of results plus the totals needed to page through the rest.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public static class PagingExtensions
{
    /// <summary>Counts, then fetches one page. The query must already be ordered.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, ListQuery list, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip((list.Page - 1) * list.PageSize).Take(list.PageSize).ToListAsync(ct);
        return new PagedResult<T>(items, list.Page, list.PageSize, total);
    }
}

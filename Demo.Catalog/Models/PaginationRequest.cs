using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Models;

/// <summary>
/// Represents a request for paginated results.
/// </summary>
public class PaginationRequest(
    int pageSize = 10,
    int pageIndex = 0
)
{
    /// <summary>
    /// Number of items to include per page.
    /// </summary>
    [FromQuery(Name = "pageSize")]
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; } = pageSize;

    /// <summary>
    /// Zero-based index of the page to retrieve.
    /// </summary>
    [FromQuery(Name = "pageIndex")]
    [Range(0, int.MaxValue, ErrorMessage = "Page index must be a non-negative integer.")]
    public int PageIndex { get; set; } = pageIndex;
}

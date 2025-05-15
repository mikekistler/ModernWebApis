using Microsoft.AspNetCore.Mvc;

namespace Catalog.Models;

public class PaginationRequest(
    int pageSize = 10,
    int pageIndex = 0
)
{
    // Number of items to include per page
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = pageSize;

    // Zero-based index of the page to retrieve
    [FromQuery(Name = "pageIndex")]
    public int PageIndex { get; set; } = pageIndex;
}

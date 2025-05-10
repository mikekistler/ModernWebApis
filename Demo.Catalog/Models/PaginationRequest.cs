/// <summary>
/// Represents a request for paginated results.
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Number of items to include per page.
    /// </summary>
    /// <remarks>Defaults to 10 if not specified.</remarks>
    public int? PageSize { get; set; } = 10;

    /// <summary>
    /// Zero-based index of the page to retrieve.
    /// </summary>
    /// <remarks>Defaults to 0 if not specified.</remarks>
    public int? PageIndex { get; set; } = 0;
}
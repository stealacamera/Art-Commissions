namespace ArtCommissions.Common.DTOs.ViewModels;

public class PaginatedList<T> where T : class
{
    public List<T> Items { get; set; } = null!;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}

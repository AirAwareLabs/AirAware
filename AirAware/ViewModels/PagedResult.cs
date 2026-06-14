namespace AirAware.ViewModels;

public class PagedResult<T>
{
    public required IReadOnlyList<T> Data { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
}

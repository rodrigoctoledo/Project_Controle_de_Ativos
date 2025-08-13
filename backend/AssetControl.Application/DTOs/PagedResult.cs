namespace AssetControl.Application.DTOs;

public class PagedResult<T>
{
    public required IEnumerable<T> Items { get; set; }
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

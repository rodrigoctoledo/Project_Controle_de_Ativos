namespace AssetControl.Application.DTOs;

public class AssetQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string SortBy { get; set; } = "name"; // name|code|status|createdAt|updatedAt
    public string SortDir { get; set; } = "asc"; // asc|desc
}

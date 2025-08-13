namespace AssetControl.Domain;

public enum AssetStatus
{
    Available = 0,
    InUse = 1
}

public class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public AssetStatus Status { get; set; } = AssetStatus.Available;
    public string? CheckedOutBy { get; set; }
    public string? Notes { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

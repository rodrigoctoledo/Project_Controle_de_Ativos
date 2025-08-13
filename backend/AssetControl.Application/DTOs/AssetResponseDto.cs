using AssetControl.Domain;

namespace AssetControl.Application.DTOs;

public class AssetResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public AssetStatus Status { get; set; }
    public string? CheckedOutBy { get; set; }
    public string? Notes { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

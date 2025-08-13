namespace AssetControl.Application.DTOs;

public class CheckoutDto
{
    public string TakenBy { get; set; } = default!;
    public string? Note { get; set; }
}

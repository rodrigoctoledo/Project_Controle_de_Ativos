using AssetControl.Application.DTOs;
using AssetControl.Application.Services;
using AssetControl.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetControl.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _service;
    public AssetsController(IAssetService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AssetResponseDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDir = "asc",
        CancellationToken ct = default)
    {
        var qp = new AssetQueryParams { Page = page, PageSize = pageSize, Search = search, SortBy = sortBy, SortDir = sortDir };
        var result = await _service.ListAsync(qp, ct);
        return Ok(new PagedResult<AssetResponseDto>
        {
            Items = result.Items.Select(MapToDto),
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetResponseDto>> Get(int id, CancellationToken ct)
    {
        var entity = await _service.GetAsync(id, ct);
        if (entity is null) return NotFound(new { error = "Ativo não encontrado." });
        return Ok(MapToDto(entity));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<AssetResponseDto>> Create([FromBody] AssetCreateDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AssetResponseDto>> Update(int id, [FromBody] AssetUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, dto, ct);
        if (updated is null) return NotFound(new { error = "Ativo não encontrado." });
        return Ok(MapToDto(updated));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/checkout")]
    public async Task<ActionResult<AssetResponseDto>> Checkout(int id, [FromBody] CheckoutDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await _service.CheckoutAsync(id, dto, ct);
            if (updated is null) return NotFound(new { error = "Ativo não encontrado." });
            return Ok(MapToDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/checkin")]
    public async Task<ActionResult<AssetResponseDto>> Checkin(int id, CancellationToken ct)
    {
        try
        {
            var updated = await _service.CheckinAsync(id, ct);
            if (updated is null) return NotFound(new { error = "Ativo não encontrado." });
            return Ok(MapToDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        if (!ok) return NotFound(new { error = "Ativo não encontrado." });
        return NoContent();
    }

    private static AssetResponseDto MapToDto(Asset a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Code = a.Code,
        Status = a.Status,
        CheckedOutBy = a.CheckedOutBy,
        Notes = a.Notes,
        CheckedOutAt = a.CheckedOutAt,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}

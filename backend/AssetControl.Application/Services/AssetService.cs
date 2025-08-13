using AssetControl.Application.Abstractions;
using AssetControl.Application.DTOs;
using AssetControl.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssetControl.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAppDbContext _db;
    public AssetService(IAppDbContext db) => _db = db;

    public async Task<PagedResult<Asset>> ListAsync(AssetQueryParams qp, CancellationToken ct = default)
    {
        var query = _db.Assets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var s = qp.Search.Trim().ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(s) || a.Code.ToLower().Contains(s));
        }

        query = (qp.SortBy.ToLower(), qp.SortDir.ToLower()) switch
        {
            ("code", "desc") => query.OrderByDescending(a => a.Code),
            ("code", _) => query.OrderBy(a => a.Code),
            ("status", "desc") => query.OrderByDescending(a => a.Status),
            ("status", _) => query.OrderBy(a => a.Status),
            ("createdat", "desc") => query.OrderByDescending(a => a.CreatedAt),
            ("createdat", _) => query.OrderBy(a => a.CreatedAt),
            ("updatedat", "desc") => query.OrderByDescending(a => a.UpdatedAt),
            ("updatedat", _) => query.OrderBy(a => a.UpdatedAt),
            ("name", "desc") => query.OrderByDescending(a => a.Name),
            _ => query.OrderBy(a => a.Name)
        };

        var total = await query.CountAsync(ct);
        var page = Math.Max(1, qp.Page);
        var pageSize = Math.Clamp(qp.PageSize, 1, 100);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<Asset>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<Asset?> GetAsync(int id, CancellationToken ct = default) =>
        _db.Assets.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Asset> CreateAsync(AssetCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _db.Assets.AnyAsync(a => a.Code == dto.Code, ct);
        if (exists) throw new InvalidOperationException("Código já cadastrado.");

        var entity = new Asset
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            Status = AssetStatus.Available
        };

        _db.Assets.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Asset?> UpdateAsync(int id, AssetUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await GetAsync(id, ct);
        if (entity is null) return null;

        entity.Name = dto.Name.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetAsync(id, ct);
        if (entity is null) return false;

        _db.Assets.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Asset?> CheckoutAsync(int id, CheckoutDto dto, CancellationToken ct = default)
    {
        var entity = await GetAsync(id, ct);
        if (entity is null) return null;
        if (entity.Status == AssetStatus.InUse)
            throw new InvalidOperationException("Ativo já está em uso.");

        entity.Status = AssetStatus.InUse;
        entity.CheckedOutBy = dto.TakenBy.Trim();
        entity.Notes = dto.Note?.Trim();
        entity.CheckedOutAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Asset?> CheckinAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetAsync(id, ct);
        if (entity is null) return null;
        if (entity.Status == AssetStatus.Available)
            throw new InvalidOperationException("Ativo já está disponível.");

        entity.Status = AssetStatus.Available;
        entity.CheckedOutBy = null;
        entity.Notes = null;
        entity.CheckedOutAt = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return entity;
    }
}

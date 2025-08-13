using AssetControl.Domain;
using AssetControl.Application.DTOs;

namespace AssetControl.Application.Services;

public interface IAssetService
{
    Task<PagedResult<Asset>> ListAsync(AssetQueryParams qp, CancellationToken ct = default);
    Task<Asset?> GetAsync(int id, CancellationToken ct = default);
    Task<Asset> CreateAsync(AssetCreateDto dto, CancellationToken ct = default);
    Task<Asset?> UpdateAsync(int id, AssetUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<Asset?> CheckoutAsync(int id, CheckoutDto dto, CancellationToken ct = default);
    Task<Asset?> CheckinAsync(int id, CancellationToken ct = default);
}

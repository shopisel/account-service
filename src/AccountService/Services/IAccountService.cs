using AccountService.Contracts;

namespace AccountService.Services;

public interface IAccountService
{
    Task EnsureAccountExistsAsync(string accountId, CancellationToken ct);
    Task<List<FavoriteProductResponse>> GetFavoritesAsync(string accountId, CancellationToken ct);
    Task AddFavoriteAsync(string accountId, string productId, CancellationToken ct);
    Task<bool> RemoveFavoriteAsync(string accountId, string productId, CancellationToken ct);
    Task UpsertPushTokenAsync(string accountId, PushTokenUpsertRequest request, CancellationToken ct);
}

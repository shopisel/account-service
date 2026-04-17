using AccountService.Contracts;
using AccountService.Data;
using AccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Services;

public sealed class AccountService : IAccountService
{
    private readonly AccountServiceDbContext _dbContext;

    public AccountService(AccountServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureAccountExistsAsync(string accountId, CancellationToken ct)
    {
        var exists = await _dbContext.Accounts.AnyAsync(a => a.Id == accountId, ct);
        if (!exists)
        {
            _dbContext.Accounts.Add(new AccountEntity { Id = accountId });
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<List<FavoriteProductResponse>> GetFavoritesAsync(string accountId, CancellationToken ct)
    {
        return await _dbContext.FavoriteProducts
            .Where(f => f.AccountId == accountId)
            .Select(f => new FavoriteProductResponse(f.ProductId))
            .ToListAsync(ct);
    }

    public async Task AddFavoriteAsync(string accountId, string productId, CancellationToken ct)
    {
        await EnsureAccountExistsAsync(accountId, ct);

        var exists = await _dbContext.FavoriteProducts
            .AnyAsync(f => f.AccountId == accountId && f.ProductId == productId, ct);

        if (!exists)
        {
            _dbContext.FavoriteProducts.Add(new FavoriteProductEntity
            {
                AccountId = accountId,
                ProductId = productId
            });
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> RemoveFavoriteAsync(string accountId, string productId, CancellationToken ct)
    {
        var favorite = await _dbContext.FavoriteProducts
            .FirstOrDefaultAsync(f => f.AccountId == accountId && f.ProductId == productId, ct);

        if (favorite != null)
        {
            _dbContext.FavoriteProducts.Remove(favorite);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        return false;
    }
}

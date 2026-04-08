using AccountService.Contracts;
using AccountService.Data;
using AccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Services;

public sealed class AccountProfileService(AccountServiceDbContext dbContext) : IAccountProfileService
{
    public async Task<AccountProfileResponse> GetOrCreateAsync(
        AuthenticatedAccountContext account,
        CancellationToken ct)
    {
        var profile = await dbContext.Profiles.SingleOrDefaultAsync(x => x.Id == account.Id, ct);
        if (profile is null)
        {
            profile = CreateProfile(account);
            dbContext.Profiles.Add(profile);
        }
        else
        {
            SyncIdentityFields(profile, account);
        }

        await dbContext.SaveChangesAsync(ct);
        return Map(profile);
    }

    public async Task<AccountProfileResponse> UpdateAsync(
        AuthenticatedAccountContext account,
        UpdateAccountProfileRequest request,
        CancellationToken ct)
    {
        Validate(request);

        var profile = await dbContext.Profiles.SingleOrDefaultAsync(x => x.Id == account.Id, ct);
        if (profile is null)
        {
            profile = CreateProfile(account);
            dbContext.Profiles.Add(profile);
        }
        else
        {
            SyncIdentityFields(profile, account);
        }

        profile.DisplayName = NormalizeOptional(request.DisplayName);
        profile.PreferredStoreId = NormalizeOptional(request.PreferredStoreId);
        profile.ShoppingRadiusKm = request.ShoppingRadiusKm ?? profile.ShoppingRadiusKm;
        profile.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Map(profile);
    }

    private static AccountProfile CreateProfile(AuthenticatedAccountContext account)
    {
        var now = DateTime.UtcNow;
        return new AccountProfile
        {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            FullName = account.FullName,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static void SyncIdentityFields(AccountProfile profile, AuthenticatedAccountContext account)
    {
        profile.Username = account.Username;
        profile.Email = account.Email;
        profile.FullName = account.FullName;
        profile.UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(UpdateAccountProfileRequest request)
    {
        if (request.ShoppingRadiusKm is < 0 or > 500)
        {
            throw new ArgumentException(
                "ShoppingRadiusKm must be between 0 and 500.",
                nameof(request.ShoppingRadiusKm));
        }

        if (request.DisplayName?.Trim().Length > 128)
        {
            throw new ArgumentException(
                "DisplayName cannot exceed 128 characters.",
                nameof(request.DisplayName));
        }

        if (request.PreferredStoreId?.Trim().Length > 128)
        {
            throw new ArgumentException(
                "PreferredStoreId cannot exceed 128 characters.",
                nameof(request.PreferredStoreId));
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AccountProfileResponse Map(AccountProfile profile) =>
        new(
            profile.Id,
            profile.Username,
            profile.Email,
            profile.FullName,
            profile.DisplayName,
            profile.PreferredStoreId,
            profile.ShoppingRadiusKm,
            profile.CreatedAt,
            profile.UpdatedAt);
}

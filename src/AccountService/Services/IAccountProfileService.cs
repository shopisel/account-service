using AccountService.Contracts;

namespace AccountService.Services;

public interface IAccountProfileService
{
    Task<AccountProfileResponse> GetOrCreateAsync(AuthenticatedAccountContext account, CancellationToken ct);
    Task<AccountProfileResponse> UpdateAsync(
        AuthenticatedAccountContext account,
        UpdateAccountProfileRequest request,
        CancellationToken ct);
}

using System.Security.Claims;

namespace AccountService.Services;

public sealed record AuthenticatedAccountContext(
    string Id,
    string Username,
    string? Email,
    string? FullName)
{
    public static AuthenticatedAccountContext? FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        var id = principal.FindFirst("sub")?.Value;
        var username = principal.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return new AuthenticatedAccountContext(
            id.Trim(),
            username.Trim(),
            principal.FindFirst("email")?.Value?.Trim(),
            principal.FindFirst("name")?.Value?.Trim());
    }
}

using System.Net;
using System.Net.Http.Json;

namespace AccountService.Tests;

public class AccountsApiTests(AccountServiceApiFactory factory) : IClassFixture<AccountServiceApiFactory>
{
    [Fact]
    public async Task GetMyAccount_CreatesProfileFromClaims()
    {
        using var client = CreateClientForUser("get-profile-user", name: "Get Profile User");
        var response = await client.GetAsync("/accounts/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<AccountProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("get-profile-user", profile!.Id);
        Assert.Equal("get-profile-user", profile.Username);
        Assert.Equal("get-profile-user@shopisel.test", profile.Email);
        Assert.Equal("Get Profile User", profile.FullName);
        Assert.Equal(5m, profile.ShoppingRadiusKm);
    }

    [Fact]
    public async Task UpdateMyAccount_PersistsCustomPreferences()
    {
        using var client = CreateClientForUser("update-profile-user");
        var updateResponse = await client.PutAsJsonAsync("/accounts/me", new
        {
            displayName = "Martim",
            preferredStoreId = "continente-colombo",
            shoppingRadiusKm = 12.5m
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<AccountProfileResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Martim", updated!.DisplayName);
        Assert.Equal("continente-colombo", updated.PreferredStoreId);
        Assert.Equal(12.5m, updated.ShoppingRadiusKm);

        var getResponse = await client.GetAsync("/accounts/me");
        var fetched = await getResponse.Content.ReadFromJsonAsync<AccountProfileResponse>();

        Assert.NotNull(fetched);
        Assert.Equal("Martim", fetched!.DisplayName);
        Assert.Equal("continente-colombo", fetched.PreferredStoreId);
        Assert.Equal(12.5m, fetched.ShoppingRadiusKm);
    }

    [Fact]
    public async Task Profiles_AreIsolatedPerAuthenticatedUser()
    {
        using var client = CreateClientForUser("main-user");
        var firstUpdate = await client.PutAsJsonAsync("/accounts/me", new
        {
            displayName = "Main User",
            preferredStoreId = "store-a",
            shoppingRadiusKm = 8m
        });

        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);

        using var otherUserClient = CreateClientForUser("other-user", "other.username", "other@shopisel.test", "Other User");

        var otherResponse = await otherUserClient.GetAsync("/accounts/me");
        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);

        var otherProfile = await otherResponse.Content.ReadFromJsonAsync<AccountProfileResponse>();
        Assert.NotNull(otherProfile);
        Assert.Equal("other-user", otherProfile!.Id);
        Assert.Equal("other.username", otherProfile.Username);
        Assert.Null(otherProfile.DisplayName);
        Assert.Equal(5m, otherProfile.ShoppingRadiusKm);
    }

    [Fact]
    public async Task UpdateMyAccount_WithInvalidRadius_ReturnsValidationProblem()
    {
        using var client = CreateClientForUser("invalid-radius-user");
        var response = await client.PutAsJsonAsync("/accounts/me", new
        {
            shoppingRadiusKm = -1m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private HttpClient CreateClientForUser(
        string userId,
        string? username = null,
        string? email = null,
        string? name = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", userId);
        client.DefaultRequestHeaders.Add("X-Test-Username", username ?? userId);
        client.DefaultRequestHeaders.Add("X-Test-Email", email ?? $"{userId}@shopisel.test");
        client.DefaultRequestHeaders.Add("X-Test-Name", name ?? userId);
        return client;
    }

    private sealed record AccountProfileResponse(
        string Id,
        string Username,
        string? Email,
        string? FullName,
        string? DisplayName,
        string? PreferredStoreId,
        decimal ShoppingRadiusKm,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}

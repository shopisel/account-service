using System.Net;
using System.Net.Http.Json;

namespace AccountService.Tests;

public class AccountsApiTests(AccountServiceApiFactory factory) : IClassFixture<AccountServiceApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetMyAccount_CreatesProfileFromClaims()
    {
        var response = await _client.GetAsync("/accounts/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<AccountProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("integration-test-user", profile!.Id);
        Assert.Equal("integration-test-user", profile.Username);
        Assert.Equal("integration-test-user@shopisel.test", profile.Email);
        Assert.Equal("Integration Test User", profile.FullName);
        Assert.Equal(5m, profile.ShoppingRadiusKm);
    }

    [Fact]
    public async Task UpdateMyAccount_PersistsCustomPreferences()
    {
        var updateResponse = await _client.PutAsJsonAsync("/accounts/me", new
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

        var getResponse = await _client.GetAsync("/accounts/me");
        var fetched = await getResponse.Content.ReadFromJsonAsync<AccountProfileResponse>();

        Assert.NotNull(fetched);
        Assert.Equal("Martim", fetched!.DisplayName);
        Assert.Equal("continente-colombo", fetched.PreferredStoreId);
        Assert.Equal(12.5m, fetched.ShoppingRadiusKm);
    }

    [Fact]
    public async Task Profiles_AreIsolatedPerAuthenticatedUser()
    {
        var firstUpdate = await _client.PutAsJsonAsync("/accounts/me", new
        {
            displayName = "Main User",
            preferredStoreId = "store-a",
            shoppingRadiusKm = 8m
        });

        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);

        using var otherUserClient = factory.CreateClient();
        otherUserClient.DefaultRequestHeaders.Add("X-Test-User", "other-user");
        otherUserClient.DefaultRequestHeaders.Add("X-Test-Username", "other.username");
        otherUserClient.DefaultRequestHeaders.Add("X-Test-Email", "other@shopisel.test");
        otherUserClient.DefaultRequestHeaders.Add("X-Test-Name", "Other User");

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
        var response = await _client.PutAsJsonAsync("/accounts/me", new
        {
            shoppingRadiusKm = -1m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

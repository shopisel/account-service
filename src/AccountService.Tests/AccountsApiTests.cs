using System.Net;
using System.Net.Http.Json;

namespace AccountService.Tests;

public class AccountsApiTests(AccountServiceApiFactory factory) : IClassFixture<AccountServiceApiFactory>
{
    [Fact]
    public async Task SyncMyAccount_CreatesAccount()
    {
        using var client = CreateClientForUser("sync-user");
        var response = await client.PostAsync("/accounts/me/sync", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ThenGetFavorites_ReturnsSavedProducts()
    {
        using var client = CreateClientForUser("favorites-user");
        var addResponse = await client.PostAsJsonAsync("/accounts/me/favorites", new { productId = "P-001" });

        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var listResponse = await client.GetAsync("/accounts/me/favorites");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var favorites = await listResponse.Content.ReadFromJsonAsync<List<FavoriteProductResponse>>();
        Assert.NotNull(favorites);
        Assert.Contains(favorites!, x => x.ProductId == "P-001");
    }

    [Fact]
    public async Task Favorites_AreIsolatedPerAuthenticatedUser()
    {
        using var mainClient = CreateClientForUser("main-user");
        var addResponse = await mainClient.PostAsJsonAsync("/accounts/me/favorites", new { productId = "main-product" });
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        using var otherUserClient = CreateClientForUser("other-user");
        var otherResponse = await otherUserClient.GetAsync("/accounts/me/favorites");
        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);

        var otherFavorites = await otherResponse.Content.ReadFromJsonAsync<List<FavoriteProductResponse>>();
        Assert.NotNull(otherFavorites);
        Assert.DoesNotContain(otherFavorites!, x => x.ProductId == "main-product");
    }

    [Fact]
    public async Task RemoveFavorite_WhenExists_ReturnsNoContent_ThenNotFound()
    {
        using var client = CreateClientForUser("remove-user");
        var addResponse = await client.PostAsJsonAsync("/accounts/me/favorites", new { productId = "P-REMOVE" });
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var firstDelete = await client.DeleteAsync("/accounts/me/favorites/P-REMOVE");
        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);

        var secondDelete = await client.DeleteAsync("/accounts/me/favorites/P-REMOVE");
        Assert.Equal(HttpStatusCode.NotFound, secondDelete.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_WithInvalidProductId_ReturnsBadRequest()
    {
        using var client = CreateClientForUser("invalid-product-user");
        var response = await client.PostAsJsonAsync("/accounts/me/favorites", new
        {
            productId = ""
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

    private sealed record FavoriteProductResponse(string ProductId);
}

using System.Security.Claims;
using AccountService.Contracts;
using AccountService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var accounts = app
            .MapGroup("/accounts")
            .WithTags("Account")
            .RequireAuthorization();

        accounts.MapPost("/me/sync", async (
            IAccountService accountService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var accountId = GetAccountId(httpContext.User);
            if (accountId is null)
            {
                return Results.Unauthorized();
            }

            await accountService.EnsureAccountExistsAsync(accountId, ct);
            return Results.Ok();
        })
        .WithName("SyncMyAccount")
        .WithSummary("Garante que a conta associada ao token existe na base de dados (utilizado no login)");

        accounts.MapGet("/me/favorites", async (
            IAccountService accountService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var accountId = GetAccountId(httpContext.User);
            if (accountId is null)
            {
                return Results.Unauthorized();
            }

            var favorites = await accountService.GetFavoritesAsync(accountId, ct);
            return Results.Ok(favorites);
        })
        .WithName("GetMyFavorites")
        .WithSummary("Obter produtos favoritos do utilizador autenticado");

        accounts.MapPost("/me/favorites", async (
            [FromBody] FavoriteProductRequest request,
            IAccountService accountService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var accountId = GetAccountId(httpContext.User);
            if (accountId is null)
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                return Results.BadRequest(new { error = "ProductId cannot be empty." });
            }

            await accountService.AddFavoriteAsync(accountId, request.ProductId, ct);
            return Results.Ok();
        })
        .WithName("AddFavoriteProduct")
        .WithSummary("Adicionar um produto aos favoritos");

        accounts.MapDelete("/me/favorites/{productId}", async (
            string productId,
            IAccountService accountService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var accountId = GetAccountId(httpContext.User);
            if (accountId is null)
            {
                return Results.Unauthorized();
            }

            var removed = await accountService.RemoveFavoriteAsync(accountId, productId, ct);
            if (removed)
            {
                return Results.NoContent();
            }
            
            return Results.NotFound();
        })
        .WithName("RemoveFavoriteProduct")
        .WithSummary("Remover um produto dos favoritos");

        static string? GetAccountId(ClaimsPrincipal principal)
        {
            return principal.FindFirst("sub")?.Value;
        }
    }
}

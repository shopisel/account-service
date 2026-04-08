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

        accounts.MapGet("/me", async (
            IAccountProfileService accountProfileService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var account = AuthenticatedAccountContext.FromClaimsPrincipal(httpContext.User);
            if (account is null)
            {
                return Results.Unauthorized();
            }

            var profile = await accountProfileService.GetOrCreateAsync(account, ct);
            return Results.Ok(profile);
        })
        .WithName("GetMyAccount")
        .WithSummary("Obter perfil do utilizador autenticado");

        accounts.MapPut("/me", async (
            [FromBody] UpdateAccountProfileRequest request,
            IAccountProfileService accountProfileService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            try
            {
                var account = AuthenticatedAccountContext.FromClaimsPrincipal(httpContext.User);
                if (account is null)
                {
                    return Results.Unauthorized();
                }

                var profile = await accountProfileService.UpdateAsync(account, request, ct);
                return Results.Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
        })
        .WithName("UpdateMyAccount")
        .WithSummary("Atualizar preferencias do perfil autenticado");
    }
}

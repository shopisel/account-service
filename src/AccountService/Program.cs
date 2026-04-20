using AccountService.Data;
using AccountService.Endpoints;
using AccountService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("AccountService");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ConnectionStrings:AccountService' is required.");
}

builder.Services.AddDbContext<AccountServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
var keycloakAudience = builder.Configuration["Keycloak:Audience"];
var keycloakRequireHttpsMetadata =
    builder.Configuration.GetValue("Keycloak:RequireHttpsMetadata", true);

if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    throw new InvalidOperationException("Configuration 'Keycloak:Authority' is required.");
}

if (string.IsNullOrWhiteSpace(keycloakAudience))
{
    throw new InvalidOperationException("Configuration 'Keycloak:Audience' is required.");
}

var normalizedAuthority = keycloakAuthority.TrimEnd('/');

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Authority = normalizedAuthority;
        options.RequireHttpsMetadata = keycloakRequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = normalizedAuthority,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var authorizedParty = context.Principal?.FindFirst("azp")?.Value;
                if (!string.Equals(authorizedParty, keycloakAudience, StringComparison.Ordinal))
                {
                    context.Fail(
                        $"Invalid 'azp' claim. Expected '{keycloakAudience}', got '{authorizedParty ?? "<missing>"}'.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAccountService, AccountService.Services.AccountService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await InitializeDatabaseAsync(app);

app.MapAccountEndpoints();

await app.RunAsync();

static async Task InitializeDatabaseAsync(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AccountServiceDbContext>();
    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
    {
        await dbContext.Database.MigrateAsync();
        return;
    }

    await dbContext.Database.EnsureCreatedAsync();
}

public partial class Program;

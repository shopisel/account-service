namespace AccountService.Contracts;

public sealed record UpdateAccountProfileRequest(
    string? DisplayName,
    string? PreferredStoreId,
    decimal? ShoppingRadiusKm);

public sealed record AccountProfileResponse(
    string Id,
    string Username,
    string? Email,
    string? FullName,
    string? DisplayName,
    string? PreferredStoreId,
    decimal ShoppingRadiusKm,
    DateTime CreatedAt,
    DateTime UpdatedAt);

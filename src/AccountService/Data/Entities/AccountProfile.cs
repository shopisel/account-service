namespace AccountService.Data.Entities;

public sealed class AccountProfile
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
    public string? PreferredStoreId { get; set; }
    public decimal ShoppingRadiusKm { get; set; } = 5m;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

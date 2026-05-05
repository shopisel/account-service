namespace AccountService.Data.Entities;

public sealed class FavoriteProductEntity
{
    public string AccountId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    
    // Navigation property
    public AccountEntity Account { get; set; } = null!;
}

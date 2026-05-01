namespace AccountService.Data.Entities;

public sealed class AccountPushTokenEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string AccountId { get; set; } = null!;

    public string FcmToken { get; set; } = null!;

    public string Platform { get; set; } = null!;

    public string? DeviceId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastSeenAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AccountEntity Account { get; set; } = null!;
}


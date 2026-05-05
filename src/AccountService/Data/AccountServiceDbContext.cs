using AccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data;

public sealed class AccountServiceDbContext(DbContextOptions<AccountServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<FavoriteProductEntity> FavoriteProducts => Set<FavoriteProductEntity>();
    public DbSet<AccountPushTokenEntity> AccountPushTokens => Set<AccountPushTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("varchar(128)");
        });

        modelBuilder.Entity<FavoriteProductEntity>(entity =>
        {
            entity.ToTable("favorite_products");
            
            // Composite primary key
            entity.HasKey(x => new { x.AccountId, x.ProductId });

            entity.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .HasColumnType("varchar(128)")
                .IsRequired();

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("varchar(128)")
                .IsRequired();

            entity.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountPushTokenEntity>(entity =>
        {
            entity.ToTable("account_push_tokens");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            entity.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .HasColumnType("varchar(128)")
                .IsRequired();

            entity.Property(x => x.FcmToken)
                .HasColumnName("fcm_token")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(x => x.Platform)
                .HasColumnName("platform")
                .HasColumnType("varchar(16)")
                .IsRequired();

            entity.Property(x => x.DeviceId)
                .HasColumnName("device_id")
                .HasColumnType("varchar(128)");

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.LastSeenAt)
                .HasColumnName("last_seen_at");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entity.HasIndex(x => x.FcmToken)
                .IsUnique();

            entity.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

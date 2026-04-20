using AccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data;

public sealed class AccountServiceDbContext(DbContextOptions<AccountServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<FavoriteProductEntity> FavoriteProducts => Set<FavoriteProductEntity>();

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
    }
}

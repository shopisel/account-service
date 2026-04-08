using AccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data;

public sealed class AccountServiceDbContext(DbContextOptions<AccountServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<AccountProfile> Profiles => Set<AccountProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var profile = modelBuilder.Entity<AccountProfile>();

        profile.ToTable("account_profiles");
        profile.HasKey(x => x.Id);
        profile.Property(x => x.Id).HasMaxLength(128);
        profile.Property(x => x.Username).HasMaxLength(128).IsRequired();
        profile.Property(x => x.Email).HasMaxLength(256);
        profile.Property(x => x.FullName).HasMaxLength(256);
        profile.Property(x => x.DisplayName).HasMaxLength(128);
        profile.Property(x => x.PreferredStoreId).HasMaxLength(128);
        profile.Property(x => x.ShoppingRadiusKm).HasPrecision(10, 2);
        profile.Property(x => x.CreatedAt).IsRequired();
        profile.Property(x => x.UpdatedAt).IsRequired();
    }
}

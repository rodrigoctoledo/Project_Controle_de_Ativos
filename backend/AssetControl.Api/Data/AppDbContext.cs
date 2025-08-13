using AssetControl.Application.Abstractions;
using AssetControl.Domain;
using AssetControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AssetControl.Api.Data;
public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<User> Users => Set<User>(); // <-- plural aqui

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====== Asset ======
        var asset = modelBuilder.Entity<Asset>();
        asset.ToTable("Assets");
        asset.HasKey(a => a.Id);
        asset.Property(a => a.Name).HasMaxLength(100).IsRequired();
        asset.Property(a => a.Code).HasMaxLength(50).IsRequired();
        asset.HasIndex(a => a.Code).IsUnique();
        asset.Property(a => a.Status).HasConversion<int>();
        asset.Property(a => a.CreatedAt).IsRequired();
        asset.Property(a => a.UpdatedAt);

        // ====== User ======
        var user = modelBuilder.Entity<User>();
        user.ToTable("Users"); // <-- plural aqui
        user.HasKey(u => u.Id);
        user.Property(u => u.Name).HasMaxLength(120).IsRequired();
        user.Property(u => u.Email).HasMaxLength(160).IsRequired();
        user.HasIndex(u => u.Email).IsUnique();
        user.Property(u => u.PasswordHash).IsRequired();
        user.Property(u => u.Role).HasMaxLength(40).HasDefaultValue("User").IsRequired();
        user.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Asset>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

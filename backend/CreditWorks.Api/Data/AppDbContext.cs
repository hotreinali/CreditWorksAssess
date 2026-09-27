using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryConfiguration> CategoryConfigurations => Set<CategoryConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                new Manufacturer { Id = 1, Name = "Mazda" },
                new Manufacturer { Id = 2, Name = "Mercedes" },
                new Manufacturer { Id = 3, Name = "Honda" },
                new Manufacturer { Id = 4, Name = "Ferrari" },
                new Manufacturer { Id = 5, Name = "Toyota" });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.WeightKg).HasPrecision(18, 2);
            entity.ToTable(t => t.HasCheckConstraint("CK_Vehicles_WeightKg", "[WeightKg] >= 0.01"));
            entity.HasOne(x => x.Manufacturer).WithMany(x => x.Vehicles)
                .HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IconKey).HasMaxLength(50).IsRequired();
            entity.Property(x => x.StartsAtKg).HasPrecision(18, 2);
            entity.HasIndex(x => x.StartsAtKg).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("CK_Categories_StartsAtKg", "[StartsAtKg] >= 0.01"));
            entity.HasData(
                new Category { Id = 1, Name = "Light", IconKey = "light", StartsAtKg = 0.01m },
                new Category { Id = 2, Name = "Medium", IconKey = "medium", StartsAtKg = 500m },
                new Category { Id = 3, Name = "Heavy", IconKey = "heavy", StartsAtKg = 2500m });
        });

        modelBuilder.Entity<CategoryConfiguration>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasData(new CategoryConfiguration { Id = 1, Version = 1 });
        });
    }
}

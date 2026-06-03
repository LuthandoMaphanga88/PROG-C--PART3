using Microsoft.EntityFrameworkCore;
using Techmove.Models;

namespace Techmove.Data;

/// <summary>
/// Entity Framework Core database context for the Techmove application.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets for each entity
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Contract> Contracts { get; set; } = null!;
    public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
    public DbSet<AppUser> AppUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Client entity
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountUsername).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactDetails).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ModifiedDate).IsRequired();

            entity.HasIndex(e => e.AccountUsername).IsUnique();

            entity.HasMany(e => e.Contracts)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Contract entity
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ClientAccountUsername).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ServiceLevel).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AgreementFileName).HasMaxLength(500);
            entity.Property(e => e.ClientReturnedAgreementFileName).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ModifiedDate).IsRequired();

            entity.HasIndex(e => e.ClientAccountUsername);

            entity.HasMany(e => e.ServiceRequests)
                .WithOne(sr => sr.Contract)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ServiceRequest entity
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractId).IsRequired();
            entity.Property(e => e.ContractRef).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.CostUsd).HasPrecision(18, 2);
            entity.Property(e => e.CostZar).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ModifiedDate).IsRequired();

            entity.HasIndex(e => e.ContractRef);
        });

        // Configure AppUser entity
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ModifiedDate).IsRequired();

            entity.HasIndex(e => e.Username).IsUnique();

            // Seed default users
            entity.HasData(
                new AppUser
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123",
                    Role = "Admin",
                    DisplayName = "System Admin",
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new AppUser
                {
                    Id = 2,
                    Username = "client",
                    Password = "client123",
                    Role = "Client",
                    DisplayName = "Client User",
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                }
            );
        });
    }
}

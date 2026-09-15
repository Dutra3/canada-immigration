using CanadaImmigration.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CanadaImmigration.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<SubscriberCategory> SubscriberCategories => Set<SubscriberCategory>();
    public DbSet<DrawCheckState> DrawCheckStates => Set<DrawCheckState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.HasIndex(s => s.Email).IsUnique();
            entity.HasIndex(s => s.UnsubscribeToken).IsUnique();
            entity.Property(s => s.Email).IsRequired();
        });

        modelBuilder.Entity<SubscriberCategory>(entity =>
        {
            entity.HasIndex(sc => new { sc.SubscriberId, sc.Category }).IsUnique();
            entity.HasOne(sc => sc.Subscriber)
                .WithMany(s => s.Categories)
                .HasForeignKey(sc => sc.SubscriberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

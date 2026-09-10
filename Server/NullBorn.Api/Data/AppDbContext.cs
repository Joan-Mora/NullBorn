using Microsoft.EntityFrameworkCore;
using NullBorn.Api.Models;

namespace NullBorn.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MapZone> MapZones => Set<MapZone>();
    public DbSet<MapMarker> MapMarkers => Set<MapMarker>();
    public DbSet<PlayerState> PlayerStates => Set<PlayerState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MapZone>(e =>
        {
            e.ToTable("map_zones");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.MapImageUrl).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<MapMarker>(e =>
        {
            e.ToTable("map_markers");
            e.HasKey(x => x.Id);
            e.Property(x => x.ZoneId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Label).HasMaxLength(128).IsRequired();
            e.Property(x => x.Color).HasMaxLength(32).IsRequired();
            e.Property(x => x.Type).HasMaxLength(32).IsRequired();
            e.HasIndex(x => x.ZoneId);
            e.HasOne(x => x.Zone)
                .WithMany(z => z.Markers)
                .HasForeignKey(x => x.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerState>(e =>
        {
            e.ToTable("player_state");
            e.HasKey(x => x.PlayerId);
            e.Property(x => x.PlayerId).HasMaxLength(64);
        });
    }
}

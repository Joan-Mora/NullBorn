using Microsoft.EntityFrameworkCore;
using NullBorn.Api.Data;
using NullBorn.Api.Models;

namespace NullBorn.Api.Data;

public static class DbSeeder
{
    public const string ArtTestZoneId = "arttest";
    public const string LocalPlayerId = "local-player";

    public static async Task SeedAsync(AppDbContext db)
    {
        var zone = await db.MapZones.FirstOrDefaultAsync(z => z.Id == ArtTestZoneId);
        if (zone is null)
        {
            db.MapZones.Add(new MapZone
            {
                Id = ArtTestZoneId,
                Name = "Art Test Plaza",
                MinX = -40f,
                MaxX = 40f,
                MinZ = -40f,
                MaxZ = 40f,
                MapImageUrl = "/maps/arttest.svg"
            });
        }
        else
        {
            zone.Name = "Art Test Plaza";
            zone.MinX = -40f;
            zone.MaxX = 40f;
            zone.MinZ = -40f;
            zone.MaxZ = 40f;
            zone.MapImageUrl = "/maps/arttest.svg";
        }

        if (!await db.PlayerStates.AnyAsync(p => p.PlayerId == LocalPlayerId))
        {
            db.PlayerStates.Add(new PlayerState
            {
                PlayerId = LocalPlayerId,
                X = 0f,
                Z = -3f,
                Yaw = 0f,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NullBorn.Api.Data;
using NullBorn.Api.Hubs;
using NullBorn.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrWhiteSpace(port))
    port = "5080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=nullborn;Username=nullborn;Password=nullborn";
if (connectionString.Contains("render.com", StringComparison.OrdinalIgnoreCase)
    && connectionString.IndexOf("SSL Mode", StringComparison.OrdinalIgnoreCase) < 0)
{
    connectionString += ";SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await EnsureSchemaAsync(db);
    await DbSeeder.SeedAsync(db);
}

app.MapHub<GameHub>(GameHub.HubPath);

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "NullBorn.Api" }));

app.MapGet("/api/zones/{id}", async (string id, AppDbContext db) =>
{
    var zone = await db.MapZones.AsNoTracking().FirstOrDefaultAsync(z => z.Id == id);
    return zone is null
        ? Results.NotFound()
        : Results.Ok(ToZoneDto(zone));
});

app.MapGet("/api/markers", async (string? zoneId, AppDbContext db) =>
{
    var query = db.MapMarkers.AsNoTracking().AsQueryable();
    if (!string.IsNullOrWhiteSpace(zoneId))
        query = query.Where(m => m.ZoneId == zoneId);

    var list = await query.OrderBy(m => m.CreatedAt).ToListAsync();
    return Results.Ok(list.Select(ToMarkerDto));
});

app.MapPost("/api/markers", async (CreateMarkerRequest req, AppDbContext db, IHubContext<GameHub> hub) =>
{
    if (string.IsNullOrWhiteSpace(req.ZoneId))
        return Results.BadRequest(new { error = "zoneId required" });

    var zoneExists = await db.MapZones.AnyAsync(z => z.Id == req.ZoneId);
    if (!zoneExists)
        return Results.NotFound(new { error = "zone not found" });

    var now = DateTimeOffset.UtcNow;
    var marker = new MapMarker
    {
        Id = Guid.NewGuid(),
        ZoneId = req.ZoneId,
        Label = string.IsNullOrWhiteSpace(req.Label) ? "Waypoint" : req.Label!.Trim(),
        X = req.X,
        Z = req.Z,
        Color = string.IsNullOrWhiteSpace(req.Color) ? "#e85d04" : req.Color!,
        Type = string.IsNullOrWhiteSpace(req.Type) ? "nav" : req.Type!,
        IsActive = false,
        CreatedAt = now,
        UpdatedAt = now
    };

    db.MapMarkers.Add(marker);
    await db.SaveChangesAsync();
    await BroadcastMarkersAsync(hub, marker.ZoneId, db);
    return Results.Created($"/api/markers/{marker.Id}", ToMarkerDto(marker));
});

app.MapPatch("/api/markers/{id:guid}", async (Guid id, PatchMarkerRequest req, AppDbContext db, IHubContext<GameHub> hub) =>
{
    var marker = await db.MapMarkers.FirstOrDefaultAsync(m => m.Id == id);
    if (marker is null)
        return Results.NotFound();

    if (req.Label is not null) marker.Label = req.Label.Trim();
    if (req.X.HasValue) marker.X = req.X.Value;
    if (req.Z.HasValue) marker.Z = req.Z.Value;
    if (req.Color is not null) marker.Color = req.Color;
    if (req.IsActive.HasValue)
    {
        if (req.IsActive.Value)
        {
            var others = await db.MapMarkers
                .Where(m => m.ZoneId == marker.ZoneId && m.Id != marker.Id && m.IsActive)
                .ToListAsync();
            foreach (var other in others)
                other.IsActive = false;
        }

        marker.IsActive = req.IsActive.Value;
    }

    marker.UpdatedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    await BroadcastMarkersAsync(hub, marker.ZoneId, db);
    return Results.Ok(ToMarkerDto(marker));
});

app.MapDelete("/api/markers/{id:guid}", async (Guid id, AppDbContext db, IHubContext<GameHub> hub) =>
{
    var marker = await db.MapMarkers.FirstOrDefaultAsync(m => m.Id == id);
    if (marker is null)
        return Results.NotFound();

    var zoneId = marker.ZoneId;
    db.MapMarkers.Remove(marker);
    await db.SaveChangesAsync();
    await BroadcastMarkersAsync(hub, zoneId, db);
    return Results.NoContent();
});

app.MapGet("/api/player/{id}", async (string id, AppDbContext db) =>
{
    var player = await db.PlayerStates.AsNoTracking().FirstOrDefaultAsync(p => p.PlayerId == id);
    return player is null
        ? Results.NotFound()
        : Results.Ok(ToPlayerDto(player));
});

app.MapPut("/api/player/{id}", async (string id, UpdatePlayerPoseRequest req, AppDbContext db, IHubContext<GameHub> hub) =>
{
    var player = await db.PlayerStates.FirstOrDefaultAsync(p => p.PlayerId == id);
    if (player is null)
    {
        player = new PlayerState { PlayerId = id };
        db.PlayerStates.Add(player);
    }

    player.X = req.X;
    player.Z = req.Z;
    player.Yaw = req.Yaw;
    player.UpdatedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();

    var dto = ToPlayerDto(player);
    await hub.Clients.All.SendAsync("PlayerPoseUpdated", dto);
    return Results.Ok(dto);
});

app.MapFallbackToFile("index.html");

app.Run();

static ZoneDto ToZoneDto(MapZone z) =>
    new(z.Id, z.Name, z.MinX, z.MaxX, z.MinZ, z.MaxZ, z.MapImageUrl);

static MarkerDto ToMarkerDto(MapMarker m) =>
    new(m.Id, m.ZoneId, m.Label, m.X, m.Z, m.Color, m.Type, m.IsActive, m.UpdatedAt);

static PlayerPoseDto ToPlayerDto(PlayerState p) =>
    new(p.PlayerId, p.X, p.Z, p.Yaw, p.UpdatedAt);

static async Task BroadcastMarkersAsync(IHubContext<GameHub> hub, string zoneId, AppDbContext db)
{
    var markers = await db.MapMarkers.AsNoTracking()
        .Where(m => m.ZoneId == zoneId)
        .OrderBy(m => m.CreatedAt)
        .ToListAsync();
    var payload = markers.Select(ToMarkerDto).ToList();
    await hub.Clients.Group(GameHub.ZoneGroup(zoneId)).SendAsync("MarkersChanged", zoneId, payload);
    await hub.Clients.All.SendAsync("MarkersChanged", zoneId, payload);
}

static async Task EnsureSchemaAsync(AppDbContext db)
{
    // Thesis-friendly bootstrap without requiring `dotnet ef` on every machine.
    await db.Database.EnsureCreatedAsync();
}

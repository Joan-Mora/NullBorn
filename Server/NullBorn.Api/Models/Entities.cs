namespace NullBorn.Api.Models;

public class MapZone
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public float MinX { get; set; }
    public float MaxX { get; set; }
    public float MinZ { get; set; }
    public float MaxZ { get; set; }
    public string MapImageUrl { get; set; } = "/maps/arttest.svg";

    public ICollection<MapMarker> Markers { get; set; } = new List<MapMarker>();
}

public class MapMarker
{
    public Guid Id { get; set; }
    public string ZoneId { get; set; } = "";
    public string Label { get; set; } = "Marker";
    public float X { get; set; }
    public float Z { get; set; }
    public string Color { get; set; } = "#e85d04";
    public string Type { get; set; } = "nav";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public MapZone? Zone { get; set; }
}

public class PlayerState
{
    public string PlayerId { get; set; } = "";
    public float X { get; set; }
    public float Z { get; set; }
    public float Yaw { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

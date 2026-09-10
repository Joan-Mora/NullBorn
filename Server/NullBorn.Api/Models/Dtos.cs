namespace NullBorn.Api.Models;

public record ZoneDto(
    string Id,
    string Name,
    float MinX,
    float MaxX,
    float MinZ,
    float MaxZ,
    string MapImageUrl);

public record MarkerDto(
    Guid Id,
    string ZoneId,
    string Label,
    float X,
    float Z,
    string Color,
    string Type,
    bool IsActive,
    DateTimeOffset UpdatedAt);

public record CreateMarkerRequest(
    string ZoneId,
    string? Label,
    float X,
    float Z,
    string? Color,
    string? Type);

public record PatchMarkerRequest(
    string? Label,
    float? X,
    float? Z,
    string? Color,
    bool? IsActive);

public record PlayerPoseDto(
    string PlayerId,
    float X,
    float Z,
    float Yaw,
    DateTimeOffset UpdatedAt);

public record UpdatePlayerPoseRequest(
    float X,
    float Z,
    float Yaw);

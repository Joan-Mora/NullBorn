using Microsoft.AspNetCore.SignalR;

namespace NullBorn.Api.Hubs;

public class GameHub : Hub
{
    public const string HubPath = "/hubs/game";

    public async Task JoinZone(string zoneId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ZoneGroup(zoneId));
    }

    public async Task LeaveZone(string zoneId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ZoneGroup(zoneId));
    }

    public static string ZoneGroup(string zoneId) => $"zone:{zoneId}";
}

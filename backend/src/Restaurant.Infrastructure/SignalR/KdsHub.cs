using Microsoft.AspNetCore.SignalR;
using Restaurant.Application.Common.Interfaces;

namespace Restaurant.Infrastructure.SignalR;

public class KdsHub : Hub<IKdsClient>
{
    public async Task JoinBranchGroup(long branchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"branch_{branchId}");
    }

    public async Task LeaveBranchGroup(long branchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch_{branchId}");
    }

    public async Task JoinStationGroup(long stationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"station_{stationId}");
    }
}

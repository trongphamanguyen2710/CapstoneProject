using Microsoft.AspNetCore.SignalR;

namespace DrowsinessDetectionServer.SignalR;

public class ChatHub : Hub
{
    public static readonly Dictionary<long, string> ClientConnections = [];

    public override Task OnConnectedAsync()
    {
        string? userIdString = Context.GetHttpContext()?.Request.Query["userId"];
        if (long.TryParse(userIdString, out long userId)) ClientConnections[userId] = Context.ConnectionId;
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        KeyValuePair<long, string> entry = ClientConnections.FirstOrDefault(pair => pair.Value == Context.ConnectionId);
        if (entry.Key != 0) ClientConnections.Remove(entry.Key);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToUser(long userId, string message)
    {
        if (ClientConnections.TryGetValue(userId, out string? connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }
    }
}

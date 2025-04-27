using DrowsinessDetectionServer.Models.MessageModel;
using DrowsinessDetectionServer.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Controllers;

[Route("drownsiness/api/message")]
[ApiController]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageController(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost]
    [Route("send")]
    public async Task<IActionResult> Send(SendMessageRequest request)
    {
        if (ChatHub.ClientConnections.TryGetValue(request.TargetUserId, out string? connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", request.Message);
            return Ok("Message sent to client.");
        }

        return NotFound("User not connected.");
    }
}

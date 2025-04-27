namespace DrowsinessDetectionServer.Models.MessageModel;

public class SendMessageRequest
{
    public long TargetUserId { get; set; } = -1;
    public string Message { get; set; } = string.Empty;
}

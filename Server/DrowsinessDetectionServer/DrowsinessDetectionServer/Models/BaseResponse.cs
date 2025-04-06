namespace DrowsinessDetectionServer.Models;

public class BaseResponse
{
    public int StatusCode { get; set; } = 500;
    public string Message { get; set; } = string.Empty;
}

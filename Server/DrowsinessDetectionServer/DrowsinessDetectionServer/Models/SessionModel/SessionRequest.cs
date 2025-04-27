namespace DrowsinessDetectionServer.Models.SessionModel;

public class SessionStartRequest
{
    public long DriverId { get; set; }
}

public class SessionEndRequest
{
    public long SessionId { get; set; }
}

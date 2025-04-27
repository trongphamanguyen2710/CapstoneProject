using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.SessionModel;

public class SessionGetResponse : BaseResponse
{
    public MonitorSession? Session { get; set; }
}

public class SessionGetListResponse : BaseResponse
{
    public List<MonitorSession>? Sessions { get; set; }
}

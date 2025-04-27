using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.NotificationModel;

public class NotificationGetResponse : BaseResponse
{
    public NotificationLog? Notification { get; set; }
}

public class NotificationGetListResponse : BaseResponse
{
    public List<NotificationLog>? Notifications { get; set; }
}

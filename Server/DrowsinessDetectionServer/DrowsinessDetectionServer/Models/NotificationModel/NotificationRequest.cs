using DrowsinessDetectionServer.Enum;

namespace DrowsinessDetectionServer.Models.NotificationModel;

public class NotificationCreateRequest
{
    public long DetectionId { get; set; }
    public long SupervisorId { get; set; }
}

public class NotificationChangeStatusRequest
{
    public long NotificationsId { get; set; }
    public NotificationStatus Status { get; set; }
}

using DrowsinessDetectionServer.Enum;
using DrowsinessDetectionServer.Models.NotificationModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class NotificationLog : ICommonField
{
    [Key]
    public long Id { get; set; }
    public DateTime TimeStamp { get; set; }
    public NotificationStatus Status { get; set; }

    [ForeignKey(nameof(Supervisor))]
    public long SupervisorId { get; set; }
    [JsonIgnore]
    public virtual Supervisor Supervisor { get; set; } = null!;

    [ForeignKey(nameof(Detection))]
    public long DetectionId { get; set; }
    [JsonIgnore]
    public virtual DetectionLog Detection { get; set; } = null!;

    public NotificationLog()
    {

    }

    public NotificationLog(DetectionLog detection, Supervisor supervisor)
    {
        Detection = detection;
        Supervisor = supervisor;
        TimeStamp = DateTime.Now;
        Status = NotificationStatus.Created;
    }
}

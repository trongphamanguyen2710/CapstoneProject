using DrowsinessDetectionServer.Enum;
using DrowsinessDetectionServer.Models.DetectionModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class DetectionLog : ICommonField
{
    [Key]
    public long Id { get; set; }
    public DateTime TimeStamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public DectectionType Type { get; set; }
    public DetectionStatus Status { get; set; }

    [ForeignKey(nameof(Driver))]
    public long DriverId { get; set; }
    [JsonIgnore]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey(nameof(Session))]
    public long SessionId { get; set; }
    [JsonIgnore]
    public virtual MonitorSession Session { get; set; } = null!;

    public ICollection<NotificationLog>? NotificationLogs { get; set; }

    public DetectionLog()
    {

    }

    public DetectionLog(DetectionCreateRequest request, MonitorSession session, Driver driver)
    {
        TimeStamp = DateTime.Now;
        Location = request.Location;
        Type = request.Type;
        Status = DetectionStatus.Detected;
        Driver = driver;
        Session = session;
    }

    public void Response()
    {
        Status = DetectionStatus.Responded;
    }
}

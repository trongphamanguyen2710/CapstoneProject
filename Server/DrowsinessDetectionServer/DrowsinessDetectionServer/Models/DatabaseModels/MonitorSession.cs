using DrowsinessDetectionServer.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class MonitorSession : ICommonField
{
    [Key]
    public long Id { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public MonitorStatus Status { get; set; } = MonitorStatus.Ongoing;

    [ForeignKey(nameof(Driver))]
    public long DriverId { get; set; }
    [JsonIgnore]
    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<DetectionLog>? DetectionLogs { get; set; }

    public MonitorSession()
    {

    }

    public MonitorSession(Driver driver)
    {
        StartTime = DateTime.Now;
        EndTime = null;
        Status = MonitorStatus.Ongoing;
        Driver = driver;
    }

    public void End()
    {
        EndTime = DateTime.Now;
        Status = MonitorStatus.Finished;
    }
}

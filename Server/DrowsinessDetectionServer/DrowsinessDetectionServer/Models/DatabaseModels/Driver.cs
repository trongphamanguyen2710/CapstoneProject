using DrowsinessDetectionServer.Models.AuthenticateModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class Driver : User
{
    [ForeignKey(nameof(Supervisor))]
    public long? SupervisorId { get; set; }
    [JsonIgnore]
    public virtual Supervisor? Supervisor { get; set; }
    [JsonIgnore]
    public virtual FaceData? FaceData { get; set; }

    public virtual ICollection<MonitorSession>? MonitorSessions { get; set; }
    public virtual ICollection<DetectionLog>? DetectionLogs { get; set; }

    public Driver()
    {

    }

    public Driver(RegisterRequest request)
    {
        UserName = request.UserName;
        Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Role = request.Role;
    }
}

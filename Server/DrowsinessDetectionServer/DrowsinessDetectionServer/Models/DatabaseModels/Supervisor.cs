using DrowsinessDetectionServer.Models.AuthenticateModels;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class Supervisor : User
{
    public ICollection<Driver>? Drivers { get; set; }
    public ICollection<NotificationLog>? NotificationLogs { get; set; }

    public Supervisor()
    {

    }

    public Supervisor(RegisterRequest request)
    {
        UserName = request.UserName;
        Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Role = request.Role;
    }
}

namespace DrowsinessDetectionServer.Models.UserModels;

public class ChangeUserInfoRequest
{
    public long UserId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDay { get; set; }
}

public class AssignSupervisorRequest
{
    public long DriverId { get; set; }
    public long SupervisorId { get; set; }
}

using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.UserModels;

public class UserGetResponse : BaseResponse
{
    public User? User { get; set; }
}

public class UserGetListResponse : BaseResponse
{
    public List<User>? Users { get; set; }
}

public class DriverSupervisorResponse : BaseResponse
{
    public Driver? Driver { get; set; }
    public Supervisor? Supervisor { get; set; }
}

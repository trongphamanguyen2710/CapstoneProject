using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.UserModels;

public class UserGetResponse : BaseResponse
{
    public User? User { get; set; }
}

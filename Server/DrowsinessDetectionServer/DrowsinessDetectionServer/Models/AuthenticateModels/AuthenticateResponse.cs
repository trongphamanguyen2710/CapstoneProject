using DrowsinessDetectionServer.Models.DatabaseModels;

namespace DrowsinessDetectionServer.Models.AuthenticateModels;

public class LoginResponse : BaseResponse
{
    public User? User { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class ResetPasswordResponse : BaseResponse
{
    public string ResetPassword { get; set; } = string.Empty;
}

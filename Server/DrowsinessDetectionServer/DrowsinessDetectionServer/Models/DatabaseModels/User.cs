using DrowsinessDetectionServer.Models.AuthenticateModels;
using RoleBaseAuthorizationLibrary;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class User : ICommonField
{
    [Key]
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Guest;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore] public string Password { get; set; } = string.Empty;

    public User()
    {

    }

    public User(RegisterRequest request)
    {
        UserName = request.UserName;
        Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Role = request.Role;
        Email = request.Email;
    }
}

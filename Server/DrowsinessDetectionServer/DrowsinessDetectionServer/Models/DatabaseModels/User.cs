using DrowsinessDetectionServer.Models.AuthenticateModels;
using DrowsinessDetectionServer.Models.UserModels;
using RoleBaseAuthorizationLibrary;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DrowsinessDetectionServer.Models.DatabaseModels;

public class User : ICommonField
{
    [Key]
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDay { get; set; }
    public Role Role { get; set; } = Role.Guest;

    [JsonIgnore] public string Password { get; set; } = string.Empty;

    public User()
    {

    }

    public User(RegisterRequest request)
    {
        UserName = request.UserName;
        Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Role = request.Role;
    }

    public void Update(ChangeUserInfoRequest request)
    {
        LastName = request.LastName;
        MiddleName = request.MiddleName;
        FirstName = request.FirstName;
        PhoneNumber = request.PhoneNumber;
        Email = request.Email;
        BirthDay = request.BirthDay;
    }
}

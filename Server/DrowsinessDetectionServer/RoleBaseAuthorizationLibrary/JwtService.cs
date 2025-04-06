using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace RoleBaseAuthorizationLibrary;

public class JwtService : IJwtService
{
    private readonly string secret;
    private readonly int tokenValidTime;

    public JwtService(IConfiguration configuration)
    {
        secret = "MeganeSupremacy@131211#MyrrhBestGirl";
        tokenValidTime = configuration.GetSection("TokenValidTime").Get<int>();
        if (tokenValidTime < 1) tokenValidTime = 1;
    }

    public string GenerateJwtToken(AuthorizationUser user)
    {
        try
        {
            JsonWebTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secret);
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity([new Claim("id", user.Id.ToString())]),
                Expires = DateTime.UtcNow.AddDays(tokenValidTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            string tokenString = tokenHandler.CreateToken(tokenDescriptor);
            TokenObject tokenObject = new(user, tokenString, tokenDescriptor.Expires, tokenValidTime);
            AuthorizationData.ActiveToken.AddOrUpdate(tokenString, tokenObject, (key, existingToken) => tokenObject);
            return tokenString;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<long?> ValidateJwtToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            TokenObject? tokenObject = AuthorizationData.ActiveToken.Values.FirstOrDefault(x => x.Token == token && x.ExpirationTime > DateTime.UtcNow);
            if (tokenObject == null) return null;
            JsonWebTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secret);
            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            TokenValidationResult result = await tokenHandler.ValidateTokenAsync(token, validationParameters);
            if (!result.IsValid || result.SecurityToken is not JsonWebToken jwtToken) return null;
            string? userIdClaim = jwtToken.GetPayloadValue<string>("id");
            if (long.TryParse(userIdClaim, out long userId)) return userId;
            return null;
        }
        catch
        {
            return null;
        }
    }
}

public interface IJwtService
{
    public string GenerateJwtToken(AuthorizationUser user);
    Task<long?> ValidateJwtToken(string token);
}
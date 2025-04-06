namespace RoleBaseAuthorizationLibrary;

public class TokenObject
{
    public AuthorizationUser User { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime? ExpirationTime { get; set; }

    public TokenObject(AuthorizationUser user, string token, DateTime? expirationTime, int lifetime)
    {
        User = user;
        Token = token;
        ExpirationTime = expirationTime ?? DateTime.UtcNow.AddMinutes(lifetime);
        Task.Run(async () => await Deactivate(lifetime));
    }

    private async Task Deactivate(int lifetime)
    {
        try
        {
            PeriodicTimer timer = new(new TimeSpan(0, lifetime, 3));
            await timer.WaitForNextTickAsync();
            while (DateTime.UtcNow < ExpirationTime) await Task.Delay(new TimeSpan(0, 0, 3));
            bool isRemoved = false;
            int retry = 0;
            while (!isRemoved && retry < 100)
            {
                if (!AuthorizationData.ActiveToken.ContainsKey(Token)) return;
                isRemoved = AuthorizationData.ActiveToken.TryRemove(Token, out _);
                if (!isRemoved) await Task.Delay(30);
                retry++;
            }
        }
        catch
        {

        }
    }
}

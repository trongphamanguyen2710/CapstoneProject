using System.Collections.Concurrent;

namespace RoleBaseAuthorizationLibrary;

public static class AuthorizationData
{
    public static ConcurrentDictionary<string, TokenObject> ActiveToken { get; set; } = new();

    public static async Task<AuthorizationUser?> GetUserByToken(string token)
    {
        for (int i = 0; i < 30; i++)
        {
            if (ActiveToken.TryGetValue(token, out TokenObject? tokenObject)) return tokenObject.User;
            else await Task.Delay(100);
        }
        return null;
    }

    public static async Task<bool> RemoveTokenAsync(string token)
    {
        bool isRemoved = false;
        for (int i = 0; i < 30; i++)
        {
            isRemoved = ActiveToken.TryRemove(token, out _);
            if (isRemoved) break;
            else await Task.Delay(100);
        }
        return isRemoved;
    }

    public static async Task<bool> RemoveTokensAsync(long userId)
    {
        if (!ActiveToken.Values.Any(x => x.User.Id == userId)) return true;
        for (int i = 0; i < 30; i++)
        {
            bool isSuccess = true;
            foreach (TokenObject tokenObject in ActiveToken.Values)
            {
                if (tokenObject.User.Id == userId)
                {
                    bool removed = ActiveToken.TryRemove(tokenObject.Token, out _);
                    if (!removed)
                    {
                        isSuccess = false;
                        break;
                    }
                }
            }
            if (isSuccess) return true;
            await Task.Delay(100);
        }
        return false;
    }
}

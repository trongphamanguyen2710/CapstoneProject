using Microsoft.Extensions.Caching.Memory;

namespace DrowsinessDetectionServer.Datas;

public static class CacheData
{
    public static IMemoryCache Cache { get; set; } = null!;

    public const string USER_CACHE_KEY = "User";
}

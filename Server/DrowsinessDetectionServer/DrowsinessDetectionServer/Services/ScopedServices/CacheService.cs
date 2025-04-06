using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Models;
using DrowsinessDetectionServer.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Services.ScopedServices;

public class CacheService : ICacheService
{
    private readonly ApplicationDbContext dbContext;
    private readonly MemoryCacheEntryOptions cacheEntryOptions;

    public CacheService(ApplicationDbContext dbContext, IConfiguration config)
    {
        this.dbContext = dbContext;
        int cacheSlidingExpiration = config.GetSection("CacheSlidingExpiration").Get<int>();
        if (cacheSlidingExpiration < 1) cacheSlidingExpiration = 1;
        int cacheAbsoluteExpiration = config.GetSection("CacheAbsoluteExpiration").Get<int>();
        if (cacheAbsoluteExpiration < 1) cacheAbsoluteExpiration = 1;
        cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(cacheSlidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheAbsoluteExpiration));
    }

    public async Task<User?> GetCurrentUser(HttpContext httpContext, bool fromDatabase = false)
    {
        AuthorizationUser? authorizationUser = (AuthorizationUser?)httpContext.Items["AuthorizationUser"];
        if (authorizationUser == null) return null;
        return await GetByIdAsync<User>(CacheData.USER_CACHE_KEY, authorizationUser.Id, fromDatabase);
    }

    public async Task<T?> GetByIdAsync<T>(string key, long id, bool fromDatabase = false) where T : class, ICommonField
    {
        try
        {
            T? value = default;
            string cacheKey = key + id.ToString();
            if (fromDatabase)
            {
                value = await dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
                if (value != null) CacheData.Cache.Set(cacheKey, value, cacheEntryOptions);
            }
            else
            {
                value = CacheData.Cache.Get<T>(cacheKey);
                if (value == null)
                {
                    value = await dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
                    T? temp = CacheData.Cache.Get<T>(cacheKey);
                    if (temp == null && value != null) CacheData.Cache.Set(cacheKey, value, cacheEntryOptions);
                }
            }
            return value;
        }
        catch
        {
            return default;
        }
    }

    public void AddOrUpdateCacheById<T>(string key, long id, T value)
    {
        try
        {
            string cacheKey = key + id.ToString();
            CacheData.Cache.Set(cacheKey, value, cacheEntryOptions);
        }
        catch
        {

        }
    }

    public void RemoveById(string key, long id)
    {
        try
        {
            string cacheKey = key + id.ToString();
            if (CacheData.Cache.TryGetValue(cacheKey, out _)) CacheData.Cache.Remove(cacheKey);
        }
        catch
        {

        }
    }
}

public interface ICacheService
{
    Task<User?> GetCurrentUser(HttpContext httpContext, bool fromDatabase = false);
    Task<T?> GetByIdAsync<T>(string key, long id, bool fromDatabase = false) where T : class, ICommonField;
    void AddOrUpdateCacheById<T>(string key, long id, T value);
    void RemoveById(string key, long id);
}

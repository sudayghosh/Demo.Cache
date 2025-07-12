using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Demo.Cache.Extensions
{
    public static class ExtensionMethods
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public static async Task<T?> GetOrCreateAsync<T>(this IDistributedCache cache, string key
            , Func<CancellationToken, Task<T>> factory
            , DistributedCacheEntryOptions? options = default
            , CancellationToken ct = default)
        {
            var checkCache = await cache.GetStringAsync(key, ct);
            T? result;
            if (!string.IsNullOrEmpty(checkCache))
            {
                result = JsonConvert.DeserializeObject<T>(checkCache);
                if (result is not null) return result;
            }

            result = await factory(ct);
            if (result is null) return default;

            await cache.SetStringAsync(key, JsonConvert.SerializeObject(result), new DistributedCacheEntryOptions()
            {
                //AbsoluteExpiration = DateTime.Now.AddSeconds(30),
                //AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(30),
            }, ct);

            return result;
        }
    }
}

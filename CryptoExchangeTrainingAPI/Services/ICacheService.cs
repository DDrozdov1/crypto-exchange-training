using System;
using Microsoft.Extensions.Caching.Memory;

namespace CryptoExchangeTrainingAPI.Services
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan duration);
    }

    public class CacheService : ICacheService
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public T? Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public void Set<T>(string key, T value, TimeSpan duration)
        {
            _cache.Set(key, value, duration);
        }
    }
}
using System;
using System.Runtime.Caching;

namespace EDelivery.WebPortal
{
    public static class MemoryCacheExtensions
    {
        public static readonly MemoryCache AuthorizationCache = new MemoryCache("AuthorizationCache");
        public static readonly MemoryCache UserDataCache = new MemoryCache("UserDataCache");

        public static T AddOrGetExisting<T>(
            this MemoryCache memoryCache,
            string key,
            Func<T> valueFactory,
            CacheItemPolicy cacheItemPolicy)
        {
            var newValue = new Lazy<T>(valueFactory);
            var oldValue = memoryCache.AddOrGetExisting(key, newValue, cacheItemPolicy) as Lazy<T>;
            try
            {
                return (oldValue ?? newValue).Value;
            }
            catch
            {
                // Handle cached lazy exception by evicting from cache
                memoryCache.Remove(key);
                throw;
            }
        }
    }
}

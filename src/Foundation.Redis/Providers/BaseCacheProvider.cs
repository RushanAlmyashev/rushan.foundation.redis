using System;

namespace Foundation.Redis.Providers
{
    public abstract class BaseCacheProvider
    {
        protected static void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "The key should be specified");
            }
        }
    }
}

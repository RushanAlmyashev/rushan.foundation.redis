﻿using System;

namespace Rushan.Foundation.Redis.Providers.Impl
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

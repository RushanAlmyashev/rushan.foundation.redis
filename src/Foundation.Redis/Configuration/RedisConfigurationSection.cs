using System;
using System.Configuration;

namespace Foundation.Redis.Configuration
{
#if NET5_0_OR_GREATER
    public class RedisConfigurationSection 
    {
        public string ConnectionString { get; set; }     

        public TimeSpan? DefaultCacheTime { get; set; }
    }
#else
    public class RedisConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString => (string)base["connectionString"];        

        [ConfigurationProperty("defaultCacheTime", IsRequired = true)]
        public TimeSpan? DefaultCacheTime => (TimeSpan)base["defaultCacheTime"];
    }
#endif
}

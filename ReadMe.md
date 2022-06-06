Foundation.Redis
=========== 

## Summary
An assembly for iteraction with redis cluster. General CRUD operations with data, and several features for made it easy.  

## Metadology

### RedisCacheProvider
Service provides direct access to redis storage. 

### HybridCacheProvider
Ensure two level cache. 
* 1 level. Manage runtime cache. Attempt to get value from memory 
* 2 level. Manage redis cache. Attempt to get value from redis instance and fullfill memory cache 

**NB! Do not use it, if you want share data with distributed systems.**

### DistributedLockService
Service for controls starting processes in different instances.

### ISerializer
Interface, which should used if you need your personal data-serializer. In default, implemented json serializer, with general settings.

### ILogger
Interface  for logging. In default, implemented empty logger.

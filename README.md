# redis-wrap

## Usage

### Register Service 
```

services.AddRedisService(opt =>
{
    opt.ConnectionString = redisConnection;
    //Optional
    opt.DefaultQueueSize = defaultQueueSize;
    opt.DefaultExpiredDuration = TimeSpan.FromSeconds(defaultExpiration);
});
```
Sample json
```
  "Redis": {
    "Connection": "127.0.0.1:6379",
    //Optional
    "DefaultExpirationDuration": 300,
    "DefaultQueueSize": 500
  }
```
Use the following interface
```
IRedisCacheService redisCacheService
```

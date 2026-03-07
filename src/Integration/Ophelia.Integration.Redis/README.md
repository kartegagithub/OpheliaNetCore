# Ophelia Integration Redis

This module provides a robust implementation for using Redis as a distributed cache provider within the Ophelia Framework.

## 📂 Key Components

### [RedisCache.cs](./RedisCache.cs)
The main provider implementation that connects the `Ophelia.Core.Caching` abstraction to a Redis server.
- **Key Methods**: `Get`, `Set`, `Remove`, `GetAsync`, `SetAsync`.

### [RedisCacheContext.cs](./RedisCacheContext.cs)
Manages the connection state and multiplexer for Redis interactions.

### [RedisCacheOptions.cs](./RedisCacheOptions.cs)
Configuration settings for Redis, including connection strings, timeouts, and instance names.

### [Extensions.cs](./Extensions.cs) & [RedisExtensions.cs](./RedisExtensions.cs)
Extension methods to simplify the registration of Redis services in the application pipeline.

## 🚀 Configuration

To enable Redis caching in your application:

```csharp
services.AddOpheliaRedisCache(options => {
    options.ConnectionString = "localhost:6379";
    options.InstanceName = "OpheliaApp:";
});
```

---
*Powered by StackExchange.Redis.*

# Ophelia Caching

The Caching module provides a flexible abstraction layer over various caching mechanisms, allowing for both local in-memory and distributed caching strategies.

## 📂 Core Components

### [CacheFacade.cs](./CacheFacade.cs)
The primary interface for developers. It provides high-level methods to `Get`, `Set`, and `Remove` items from the configured cache.
- **Key Methods**: `Get<T>`, `Set<T>`, `Remove`, `Clear`.

### [CacheManager.cs](./CacheManager.cs)
Manages the lifecycle and configuration of cache providers. It decides whether to use local memory or a distributed system based on application settings.
- **Key Methods**: `Initialize`, `GetProvider`.

### [LocalCache.cs](./LocalCache.cs)
A wrapper around standard .NET memory caching. Ideal for single-instance applications or non-shared data.

### [SimpleCacheFacade.cs](./SimpleCacheFacade.cs)
A lightweight version of the facade for scenarios where minimal overhead is required.

## 📁 Subdirectories
- **[CacheContexts](./CacheContexts)**: Contains context-specific caching logic (e.g., Per-Request caching).
- **[DistributedCaches](./DistributedCaches)**: Implementations for external stores like Redis.

---
*Part of the Ophelia Core Framework.*

# Ophelia Reflection

The Reflection module provides high-performance wrappers around standard .NET reflection, enabling dynamic object creation, property access, and metadata inspection.

## 📂 Components

### [Accessor.cs](./Accessor.cs)
A high-performance utility for reading and writing properties/fields dynamically. It uses caching and expression trees to minimize the overhead of reflection.
- **Key Methods**: `GetValue`, `SetValue`, `GetPropertyMap`.

### [ObjectBuilder.cs](./ObjectBuilder.cs)
Simplifies the creation of objects and population of their data from various sources.
- **Key Methods**: `Build<T>`, `MapProperties`.

### [ObjectIterator.cs](./ObjectIterator.cs)
Provides mechanisms to traverse object graphs, useful for serialization, validation, or auditing.

### [InstanceFactory.cs](./InstanceFactory.cs)
A centralized factory for creating instances of types, supporting dependency injection patterns where appropriate.
- **Key Methods**: `CreateInstance`.

## ⚡ Performance Note
The framework uses internal caching for reflected metadata to ensure that subsequent calls to reflection-heavy methods (like property access in a loop) are as fast as possible.

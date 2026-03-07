# Ophelia Entity Framework Integration

Bridge logic to allow the Ophelia Data framework to work seamlessly with Microsoft Entity Framework Core.

## 📂 Components

### [DatabaseContext.cs](./DatabaseContext.cs)
The primary EF Core `DbContext` wrapper. It integrates Ophelia's entity tracking and auditing features into the EF pipeline.

### [IEntityConfigurator.cs](./IEntityConfigurator.cs)
An interface for providing custom configurations for entities (Fluent API equivalent).

### [DataConfiguration.cs](./DataConfiguration.cs)
Settings specific to the Entity Framework integration, such as connection pooling and query splitting strategies.

---
*Combining the power of Ophelia with the standard EF Core ecosystem.*

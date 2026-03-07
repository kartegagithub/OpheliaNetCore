# Ophelia Data

Ophelia Data is a robust ORM-independent data access layer that simplifies database interactions while providing advanced features like automatic entity tracking, repository patterns, and multi-provider support.

## 📂 Core Data Components

### [DataContext.cs](./DataContext.cs)
The main entry point for database operations. It manages connections, transactions, and serves as a factory for repositories.
- **Key Methods**: `GetRepository<T>`, `SaveChanges`, `GetQuery<T>`, `Create<T>`.

### [Repository.cs](./Repository.cs)
Provides a standard implementation of the Repository Pattern. It abstracts CRUD operations and query building for specific entity types.
- **Key Methods**: `GetById`, `Add`, `Update`, `Delete`, `Filter`.

### [Connection.cs](./Connection.cs)
Handles the low-level communication with the database engine. It supports various providers and ensures connections are properly pooled and disposed.
- **Key Methods**: `Open`, `Close`, `ExecuteNonQuery`, `ExecuteScalar`.

### [DatabaseTransaction.cs](./DatabaseTransaction.cs)
A wrapper for database transactions, ensuring ACID compliance across multiple operations.

## 📁 Key Subdirectories

- **[Model](./Model)**: Contains the base classes for entities (`DataEntity`) and query results (`QueryableDataSet`).
- **[Querying](./Querying)**: Logic for the internal query engine and expression tree parsing.
- **[EntityFramework](./EntityFramework)**: Integration bridge for EF Core.
- **[Extensions](./Extensions)**: Data-specific helpers for LINQ and DataTables.

## 🚀 Quick Start
```csharp
using (var context = new MyDataContext())
{
    var repository = context.GetRepository<User>();
    var user = repository.GetById(123);
}
```

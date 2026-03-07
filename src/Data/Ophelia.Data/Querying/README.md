# Ophelia Data Querying

The engine responsible for translating LINQ expressions into SQL queries and providing tools for database schema design.

## 📂 Components

### [DataDesigner.cs](./DataDesigner.cs)
A powerful tool for programmatically managing database schema, inclusive of table creation, migration logic, and metadata inspection.

### [QueryProvider.cs](./QueryProvider.cs)
The bridge between standard .NET LINQ and the Ophelia SQL generation engine. It manages the execution of `IQueryable` expressions.

## 📁 Subdirectories
- **[Query](./Query)**: Contains specific query type implementations (Select, Insert, Update, Delete).

---
*Translating intent into efficient database operations.*

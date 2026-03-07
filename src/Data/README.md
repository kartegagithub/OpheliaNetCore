# Ophelia Data Ecosystem

This directory contains the core data access library and its associated providers and utilities.

## 📂 Projects

| Project | Description |
| :--- | :--- |
| **[Ophelia.Data](./Ophelia.Data)** | The core ORM-independent engine and repository framework. |
| **[Ophelia.Data.Exporter](./Ophelia.Data.Exporter)** | Tools for exporting data to Excel, CSV, XML, etc. |
| **[Ophelia.Data.Importer](./Ophelia.Data.Importer)** | Tools for importing data from file formats into models. |
| **Providers** | Specialized connectors for different database engines: |
| - **[SQLServer](./Ophelia.Data.SQLServer)** | Microsoft SQL Server support. |
| - **[MySQL](./Ophelia.Data.MySQL)** | MySQL and MariaDB support. |
| - **[Npgsql](./Ophelia.Data.Npgsql)** | PostgreSQL support. |
| - **[Oracle](./Ophelia.Data.Oracle)** | Oracle Database support. |

## 🛠 Provider Activation

Each provider project usually contains an extension method to register itself with the `Ophelia.Data` engine.

```csharp
// Example for PostgreSQL
context.UseNpgsql();
```

---
*Refer to individual project folders for detailed documentation.*

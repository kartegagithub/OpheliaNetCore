# Ophelia Data Extensions

Advanced extension methods optimized for data access and dynamic querying.

## 📂 Core Tools

### [DataExtensions.cs](./DataExtensions.cs)
A collection of helpers for `DataTable`, `IDataReader`, and object-to-data mapping.

### [DynamicQueryable.cs](./DynamicQueryable.cs)
A powerful implementation allowing for the construction of LINQ queries using string-based predicates and sort expressions. This is essential for building dynamic filtering UIs.

### [IQueryableNoLockExtensions.cs](./IQueryableNoLockExtensions.cs)
Extensions to execute SQL queries with the `NOLOCK` hint (SQL Server specific), improving performance for read-heavy operations where dirty reads are acceptable.

---
*Expanding the horizons of standard data querying.*

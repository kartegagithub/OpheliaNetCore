# Ophelia Data Expressions

This directory contains the various LINQ expression wrappers used by the Ophelia Data query engine to build and optimize SQL statements.

## 📂 Expression Types

| Class | Purpose |
| :--- | :--- |
| **[WhereExpression.cs](./WhereExpression.cs)** | Filters the result set based on a predicate. |
| **[SelectExpression.cs](./SelectExpression.cs)** | Projects each element of a sequence into a new form. |
| **[IncludeExpression.cs](./IncludeExpression.cs)** | Specifies the related entities to include in the query results (Eager Loading). |
| **[OrderExpression.cs](./OrderExpression.cs)** | Handles sorting operations (Ascending/Descending). |
| **[GroupExpression.cs](./GroupExpression.cs)** | Groups the elements of a sequence. |
| **[InExpression.cs](./InExpression.cs)** | Optimizes `IN` clauses for database queries. |
| **[TakeExpression.cs](./TakeExpression.cs)** & **[SkipExpression.cs](./SkipExpression.cs)** | Handles pagination (OFFSET/FETCH). |

---
*The logic behind SQL translation.*

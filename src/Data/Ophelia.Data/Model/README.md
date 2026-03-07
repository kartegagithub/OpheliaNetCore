# Ophelia Data Models

The core data structures and tracking mechanisms for the Ophelia Data ORM.

## 📂 Core Models

### [DataEntity.cs](./DataEntity.cs)
The base class for all entities managed by the Ophelia Data engine. It provides the foundation for state management and primary key handling.

### [QueryableDataSet.cs](./QueryableDataSet.cs)
A high-level wrapper around LINQ expressions that allows for framework-specific query optimizations and provider-agnostic execution.

### [DataEntityTracker.cs](./DataEntityTracker.cs) & [PocoEntityTracker.cs](./PocoEntityTracker.cs)
Crucial components that monitor changes to entities. They keep track of which properties have been modified to generate optimized `UPDATE` statements.

## 📂 Extensions
- **[ModelExtensions.cs](./ModelExtensions.cs)**: Helpers for transforming models and managing entity states.
- **[QueryableDataSetExtensions.cs](./QueryableDataSetExtensions.cs)**: A massive collection of LINQ-like extension methods optimized for the Ophelia query engine.

---
*The building blocks of a high-performance data layer.*

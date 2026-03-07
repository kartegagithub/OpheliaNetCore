# Ophelia Attributes

Custom metadata attributes used to decorate types and members across the framework.

## 📂 Attributes

### [StringValueAttribute.cs](./StringValueAttribute.cs)
Allows associating a human-readable or alternative string value with an enum member or property.

```csharp
public enum Status {
    [StringValue("Active User")]
    Active = 1
}
```

# Ophelia Extensions

This directory contains a comprehensive set of extension methods that enhance the capabilities of standard .NET types. These extensions are designed to simplify common operations and provide a more fluent API.

## 📂 Featured Extension Classes

| File | Description | Key Functions |
| :--- | :--- | :--- |
| **[StringExtensions.cs](./StringExtensions.cs)** | Advanced string manipulation and validation. | `Sanitize`, `EncodeURL`, `IsEmailAddress`, `ToInt32`, `ToStringSlug`, `ToByteArray`. |
| **[DateTimeExtensions.cs](./DateTimeExtensions.cs)** | Date and time calculations, formatting, and period logic. | `StartOfDay`, `EndOfMonth`, `ToUnixTime`, `IsWeekend`, `AddWorkDays`. |
| **[IEnumerableExtensions.cs](./IEnumerableExtensions.cs)** | Collections and list helpers. | `ForEach`, `ToDataTable`, `Randomize`, `Chunk`. |
| **[ReflectionExtensions.cs](./ReflectionExtensions.cs)** | simplified metadata inspection and dynamic invocation. | `GetPropertyValue`, `SetPropertyValue`, `GetBaseTypes`, `IsNumericType`. |
| **[DataTableExtensions.cs](./DataTableExtensions.cs)** | ADO.NET DataTable to object mapping. | `ToList<T>`, `FirstRow`, `ToJSON`. |
| **[URLExtensions.cs](./URLExtensions.cs)** | URL parsing and query string management. | `AppendQueryString`, `RemoveQueryString`, `GetDomain`. |

## 🛠 Usage Example

```csharp
using Ophelia;

string myEmail = "TEST@Example.com";
if (myEmail.IsEmailAddress()) 
{
    var normalized = myEmail.NormalizeEmail();
}

DateTime now = DateTime.Now;
var startOfNextMonth = now.AddMonths(1).StartOfMonth();
```

# Ophelia Data Exporter

The data exporting module allows for easy transformation of `IEnumerable` collections or `DataTable` objects into various file formats.

## 📂 Export Formats

| Class | Format | Description |
| :--- | :--- | :--- |
| **[ExcelExporter.cs](./ExcelExporter.cs)** | XLSX | Advanced Excel exporting with support for styling and multi-sheet workbooks. |
| **[CSVExporter.cs](./CSVExporter.cs)** | CSV | Standard comma-separated values export. |
| **[XMLExporter.cs](./XMLExporter.cs)** | XML | Generic XML tree generation from data structures. |
| **[TDFExporter.cs](./TDFExporter.cs)** | TDF | Tab-delimited file export. |

## 🛠 Usage

All exporters implement the **[IExporter.cs](./IExporter.cs)** interface, ensuring a consistent API.

```csharp
var exporter = new ExcelExporter();
var bytes = exporter.Export(myList);
File.WriteAllBytes("report.xlsx", bytes);
```

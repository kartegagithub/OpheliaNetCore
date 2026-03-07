# Ophelia Data Importer

This module facilitates the ingestion of data from external files back into the application's domain models or data tables.

## 📂 Key Components

### [ExcelImporter.cs](./ExcelImporter.cs)
Provides capabilities to read Excel files and map their rows/columns to .NET objects.
- **Key Methods**: `Import`, `ReadSheet`.

### [IImporter.cs](./IImporter.cs)
The base interface for all data importers, defining the contract for processing input streams or files.

## 🛠 Usage

```csharp
var importer = new ExcelImporter();
var results = importer.Import<User>(fileStream);
```

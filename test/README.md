# Ophelia Framework Tests

This directory contains the unit and integration tests for the various modules of the Ophelia Framework.

## 📂 Test Projects

| Project | Target Module |
| :--- | :--- |
| **[OpheliaTests](./OpheliaTests)** | Tests for `Ophelia.Core` (Extensions, Utility, Reflection, etc.). |
| **[Ophelia.DataTests](./Ophelia.DataTests)** | Tests for the `Ophelia.Data` engine and repository behavior. |
| **[Ophelia.WebTests](./Ophelia.WebTests)** | Tests for web-specific helpers, routing, and UI rendering. |
| **[Ophelia.IntegrationTests](./Ophelia.IntegrationTests)** | Tests for external service connectors (AWS, Redis, GitHub, etc.). |
| **[Ophelia.DrawingTests](./Ophelia.DrawingTests)** | Tests for image processing and manipulation logic. |

## 🚀 Running Tests

You can run all tests using the .NET CLI from the root of the repository:

```bash
dotnet test
```

## 🧪 Testing Guidelines
- **Unit Tests**: Should be isolated and not require external resources (database, internet).
- **Integration Tests**: Verify end-to-end flows and may require a test environment or mocked services.
- **Naming**: Use the `[MethodName]_[Scenario]_[ExpectedResult]` pattern.

---
*Quality assurance is a core part of the Ophelia development lifecycle.*

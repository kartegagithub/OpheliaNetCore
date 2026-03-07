# Ophelia Framework

[![NuGet Version](https://img.shields.io/nuget/v/Ophelia.svg)](https://www.nuget.org/packages/Ophelia/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

**Ophelia** is a comprehensive, modular .NET framework designed to accelerate enterprise application development. Developed by [Kartega](https://www.kartega.com), it provides a robust foundation for building scalable, maintainable, and high-performance applications across various domains, including Web, Data, AI, and Integration.

## 🚀 Key Features

-   **Core Utilities**: Advanced extensions, reflection helpers, cryptography, and globalization support.
-   **Data Access Layer**: A powerful ORM-independent data layer with support for Entity Framework Core and multiple database providers (SQL Server, MySQL, PostgreSQL, Oracle).
-   **Web Integration**: Simplified web application development with specialized routing, UI helpers, and client-side integration tools.
-   **AI & LLM Services**: Ready-to-use services for Chat (OpenAI, Anthropic, Gemini), Embedding, and Vector Databases.
-   **Third-Party Integrations**: Extensive support for external services including Amazon, GitHub, Redis, LDAP, OAuth (Google, Facebook, Apple), and more.
-   **Drawing & Media**: Specialized tools for image manipulation and document processing.

## 🏗 Project Structure

The project is organized into several high-level modules:

| Module | Description |
| :--- | :--- |
| **Ophelia.Core** | The base library containing fundamental utilities, extensions, and core abstractions. |
| **Ophelia.Data** | Database abstraction layer with repository patterns and multi-provider support. |
| **Ophelia.Web** | Web-specific extensions, UI components, and service handlers. |
| **Ophelia.AI** | Lightweight integrations for Large Language Models (LLM) and Vector storage. |
| **Ophelia.Integration** | A collection of adapters for external services and APIs. |
| **Ophelia.Drawing** | Image processing and graphic utilities. |

## 📦 Installation

Ophelia is available as a set of NuGet packages. You can install the foundation package using:

```bash
dotnet add package Ophelia
```

For data access with SQL Server:

```bash
dotnet add package Ophelia.Data.SQLServer
```

## 🛠 Usage Example

### Data Repository Usage

```csharp
using Ophelia.Data;

// Example of using a repository with Ophelia.Data
public class MyService
{
    private readonly IRepository<User> _repository;

    public MyService(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<User> GetUserAsync(long id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

## 📜 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 🏢 About Kartega

Kartega Yazılım ve Danışmanlık A.Ş. is a leading software development and consultancy company based in Turkey, specializing in enterprise-grade solutions.

---
Developed with ❤️ by [Kartega](https://www.kartega.com)
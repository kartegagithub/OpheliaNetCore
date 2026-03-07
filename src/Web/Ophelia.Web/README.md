# Ophelia Web

The Web module provides specialized tools for building modern web applications and APIs with ASP.NET Core, focusing on UI generation, routing, and client-server communication.

## 📂 Components

### [Client.cs](./Client.cs)
A service-to-service communication client utility with built-in support for standard Ophelia responses and error handling.
- **Key Methods**: `PostAsync`, `GetAsync`, `PutAsync`.

### [Utility.cs](./Utility.cs)
Web-specific utility functions including script handling, cookie management, and query string manipulation for the web context.

## 📁 Subdirectories

### [UI](./UI)
Contains visual component logic and HTML generators.
- **Controls**: Specialized inputs and containers.
- **TagHelpers**: ASP.NET Core TagHelper implementations for the framework.

### [View](./View)
Base classes and extensions for ViewModels and rendering logic.

### [Routing](./Routing)
Custom routing constraints and URL generation helpers designed for SEO and dynamic path management.

### [Extensions](./Extensions)
Extensions for `HttpContext`, `HttpRequest`, and `IServiceCollection` to wire up Ophelia services in `Program.cs`.

## 🌐 Integration
To enable Ophelia Web in your project:
```csharp
builder.Services.AddOpheliaWeb();
```
---
*Part of the Ophelia Web Framework.*

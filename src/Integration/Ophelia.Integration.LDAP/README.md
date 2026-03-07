# Ophelia Integration LDAP

This module provides connectivity to LDAP (Lightweight Directory Access Protocol) servers and Active Directory (AD) for user authentication and directory lookups.

## 📂 Core Components

### [ADFacade.cs](./ADFacade.cs)
A high-level facade for interacting with Active Directory. It simplifies common tasks like user validation and searching for directory objects.
- **Key Methods**: `ValidateUser`, `GetUserDetails`, `SearchUsers`, `ResetPassword`.

### [ADProperties.cs](./ADProperties.cs)
A collection of constants and helper methods for mapping standard Active Directory attributes (e.g., `sAMAccountName`, `mail`, `distinguishedName`).

### [Extensions.cs](./Extensions.cs)
Extensions for converting LDAP search results into typed objects or dictionary-like structures.

## 📁 Subdirectories
- **[Windows](./Windows)**: Specific implementations for Windows-based authentication environments.
- **[Web](./Web)**: Helpers for integrating LDAP authentication into web applications.

## 🚀 Usage

```csharp
var ad = new ADFacade(domain, serverIp);
bool isValid = ad.ValidateUser("username", "password");
```

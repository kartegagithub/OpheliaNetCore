# Ophelia Integration Apple Login

Enable "Sign in with Apple" capabilities in your application.

## 📂 Core Services

### [AppleLoginService.cs](./AppleLoginService.cs)
Handles the validation of identity tokens provided by Apple and retrieves user information (Email, Name) during the first-time authentication.
- **Key Methods**: `GetUserDataAsync`, `ValidateToken`.

## 📁 Subdirectories
- **[Model](./Model)**: DTOs for Apple-specific authentication payloads.

---
*Secure, privacy-focused authentication.*

# Ophelia Integration I18N Service

A client-side bridge for external Internationalization (I18N) and translation management systems.

## 📂 Core Components

### [I18NIntegratorClient.cs](./I18NIntegratorClient.cs)
The main client used to fetch localized strings, translations, and culture settings from a central I18N service provider.

### [Options.cs](./Options.cs) & [Extensions.cs](./Extensions.cs)
Configuration settings for the service endpoint and extension methods to register the client and its middleware.

## 📁 Subdirectories
- **[Middlewares](./Middlewares)**: ASP.NET Core middleware to automatically resolve culture and inject localized resources into the request context.
- **[Services](./Services)**: Internal implementations of the I18N contract.

---
*Globalizing your application at scale.*

# Ophelia Web Extensions

A comprehensive set of extension methods specifically designed for ASP.NET Core's web context.

## 📂 Context Extensions

### [HttpRequestExtension.cs](./HttpRequestExtension.cs) & [HttpResponseExtension.cs](./HttpResponseExtension.cs)
Simplified access to headers, cookies, query strings, and body content.

### [URLExtensions.cs](./URLExtensions.cs)
Helpers for building and modifying URLs within the web context.

### [SessionExtensions.cs](./SessionExtensions.cs) & [TempDataExtensions.cs](./SessionExtensions.cs)
Typed access and serialization for Session and TempData objects.

## 📂 UI & Content Extensions
- **[IFormFileExtensions.cs](./IFormFileExtensions.cs)**: Tools for handling file uploads (size checks, content types).
- **[IHtmlHelperExtensions.cs](./IHtmlHelperExtensions.cs)**: Custom rendering helpers for Razor views.
- **[IHtmlContentExtensions.cs](./IHtmlContentExtensions.cs)**: Manipulation of HTML blobs.

---
*Empowering the web pipeline.*

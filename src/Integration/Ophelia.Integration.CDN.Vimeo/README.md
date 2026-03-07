# Ophelia Integration CDN Vimeo

Integration with the Vimeo API for managing video content, uploads, and playback configurations.

## 📂 Core Services

### [VimeoClient.cs](./VimeoClient.cs)
A full-featured client for the Vimeo API. It supports video uploads (including resumable), metadata management, and privacy control.
- **Key Methods**: `UploadVideoAsync`, `GetVideoInfo`, `DeleteVideo`, `GetEmbedCode`.

### [Helpers.cs](./Helpers.cs)
Utility methods for handling Vimeo-specific authentication and URL parsing.

## 📁 Subdirectories
- **[Model](./Model)**: DTOs for Vimeo video objects, users, and API responses.

---
*Optimizing video delivery and management.*

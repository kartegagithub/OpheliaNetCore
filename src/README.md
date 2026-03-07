# Source Code Overview

This directory contains the source code for the **Ophelia Framework**. The codebase is modularized to allow developers to include only the necessary components in their projects.

## 📂 Directory Structure

### [Core](./Core)
The heartbeat of the framework.
- **Ophelia**: Base library containing absolute essentials like `Extensions`, `Cryptography`, `Cache Management`, and `Reflection` utilities.

### [Data](./Data)
Data persistence and management.
- **Ophelia.Data**: Core data abstractions and repository patterns.
- **Providers**: Specialized implementations for `SQLServer`, `MySQL`, `Npgsql` (PostgreSQL), and `Oracle`.
- **Utilities**: Data `Importer` and `Exporter` for various formats.

### [Web](./Web)
Web application development acceleration.
- **Ophelia.Web**: Helpers for ASP.NET Core, specialized routing, and UI rendering logic.

### [AI](./AI)
Artificial Intelligence and Machine Learning integrations.
- **Ophelia.AI**: abstractions for LLMs (Large Language Models) and Vector Databases. Supports OpenAI, Anthropic, and local models via ONNX.

### [Integration](./Integration)
A bridge to the outside world.
- **Social Login**: Apple, Google, Facebook.
- **Services**: Amazon AWS, GitHub, Weather APIs.
- **Infrastructure**: Redis, LDAP, Notification systems.

### [Drawing](./Drawing)
- **Ophelia.Drawing**: Utilities for image processing, resizing, and watermarking.

---
For detailed documentation on each module, please refer to the `README.md` within their respective folders.

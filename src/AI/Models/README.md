# Ophelia AI Models

Data Transfer Objects (DTOs) and configuration models used throughout the AI module.

## 📂 Data Structures

| Class | Purpose |
| :--- | :--- |
| **[AIConfig.cs](./AIConfig.cs)** | Central configuration model for AI endpoints, keys, and model parameters. |
| **[ChatHistoryMessage.cs](./ChatHistoryMessage.cs)** | Represents a single turn in a conversation (System, User, Assistant). |
| **[ChatResponse.cs](./ChatResponse.cs)** | The standardized response from an LLM. |
| **[EmbeddingResult.cs](./EmbeddingResult.cs)** | The output of an embedding operation (a list of floating-point numbers). |
| **[VectorDocument.cs](./VectorDocument.cs)** | A unit of data stored in a vector database, including its content and ID. |
| **[VectorSearchResult.cs](./VectorSearchResult.cs)** | Represents a match found during vector search, including a relevancy score. |

## 📁 Subdirectories
- **[Local](./Local)**: Models specific to local model execution (ONNX).

---
*Standardizing the AI data flow.*

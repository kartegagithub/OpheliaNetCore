# Ophelia AI

Ophelia AI provides a unified abstraction layer for integrating Large Language Models (LLM) and Vector Databases into your applications.

## 📂 Core Architecture

### [Factory.cs](./Factory.cs)
The central hub for initializing AI services. It reads configurations and returns the appropriate implementation for chat, embedding, or vector storage.
- **Key Methods**: `CreateChatService`, `CreateEmbeddingService`, `CreateVectorService`.

### [LLMType.cs](./LLMType.cs) & [VectorDbType.cs](./VectorDbType.cs)
Enums defining supported providers (e.g., OpenAI, Anthropic, Gemini for LLMs; Pinecone, Qdrant for Vector DBs).

## 📁 Key Subdirectories

- **[ChatServices](./ChatServices)**: Provider-specific implementations for conversational AI.
  - OpenAI, Anthropic, Gemini, etc.
- **[EmbeddingServices](./EmbeddingServices)**: Services to convert text/data into numerical vectors.
- **[VectorServices](./VectorServices)**: Logic for managing vector storage, similarity search, and RAG (Retrieval-Augmented Generation).
- **[Interfaces](./Interfaces)**: The architectural blueprints (`IChatService`, `IEmbeddingService`) ensuring all providers follow the same contract.

## 🤖 Example Usage
```csharp
var chatService = AIFactory.CreateChatService(LLMType.OpenAI);
var response = await chatService.AskAsync("How do I use Ophelia AI?");
```

## 🛠 Scripting
- **[download_onnx_model.ps1](./download_onnx_model.ps1)**: Utility script to download local models for offline inference.

---
*Empowering .NET applications with modern AI capabilities.*

# Ophelia AI Interfaces

The architectural blueprints for the AI module, ensuring that different providers (OpenAI, Gemini, etc.) share a common execution contract.

## 📂 Core Contracts

### [IChatService.cs](./IChatService.cs)
Defines the methods for conversational AI, including streaming support and history management.

### [IEmbeddingService.cs](./IEmbeddingService.cs)
Defines the contract for transforming data into numerical vectors.

### [IVectorStore.cs](./IVectorStore.cs)
The blueprint for vector databases, covering indexing and similarity search.

### [IChatHistoryStore.cs](./IChatHistoryStore.cs)
An interface for persisting chat messages to external stores (SQL, Redis, etc.).

---
*Ensuring provider-agnostic AI logic.*

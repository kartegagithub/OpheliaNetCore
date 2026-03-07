# Ophelia AI Embedding Services

Services for transforming text, documents, or data into numerical vector representations (embeddings) that can be understood by AI models.

## 📂 Supported Embedding Providers

- **[OpenAIEmbeddingService.cs](./OpenAIEmbeddingService.cs)**: Standard integration with OpenAI's `text-embedding-3-*` models.
- **[AzureOpenAIEmbeddingService.cs](./AzureOpenAIEmbeddingService.cs)**: Enterprise integration via Microsoft Azure.
- **[GeminiEmbeddingService.cs](./GeminiEmbeddingService.cs)**: Google's generative AI embedding models (e.g., `embedding-001`).
- **[ClaudeEmbeddingService.cs](./ClaudeEmbeddingService.cs)**: Integration for Anthropic's model suite.
- **[HuggingFaceEmbeddingService.cs](./HuggingFaceEmbeddingService.cs)**: Connects to open-source models hosted on Hugging Face Inference endpoints.
- **[LocalOnnxEmbeddingService.cs](./LocalOnnxEmbeddingService.cs)**: High-performance **offline** embeddings using local ONNX models for maximum privacy and cost efficiency.

## ⚙️ Core Logic
All services implement the shared interface to provide consistent `EmbedAsync` methods for strings and lists of strings.

---
*Converting language into mathematical insights.*

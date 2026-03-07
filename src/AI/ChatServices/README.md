# Ophelia AI Chat Services

This directory contains concrete implementations of conversational AI services for a wide variety of LLM providers.

## 📂 Supported Providers

| Implementation | Provider |
| :--- | :--- |
| **[OpenAIChatService.cs](./OpenAIChatService.cs)** | OpenAI (GPT-4, GPT-3.5) |
| **[AzureOpenAIChatService.cs](./AzureOpenAIChatService.cs)** | Microsoft Azure OpenAI Service |
| **[ClaudeChatService.cs](./ClaudeChatService.cs)** | Anthropic (Claude 3.5, 3 Opus/Sonnet) |
| **[GeminiChatService.cs](./GeminiChatService.cs)** | Google Gemini (Pro, Flash) |
| **[AmazonBedrockChatService.cs](./AmazonBedrockChatService.cs)** | Amazon Bedrock (Llama, AI21, etc.) |
| **[HuggingFaceChatService.cs](./HuggingFaceChatService.cs)** | Hugging Face Inference API |
| **[OllamaChatService.cs](./OllamaChatService.cs)** | Local models via Ollama |
| **[LMStudioChatService.cs](./LMStudioChatService.cs)** | Local models via LM Studio |

## 🏗 Base Architecture

### [BaseChatService.cs](./BaseChatService.cs)
The abstract base class that implements common logic like message history management, token estimating, and shared configuration.

### [OpenAICompatibleChatService.cs](./OpenAICompatibleChatService.cs)
A generic implementation for any provider that supports the OpenAI REST API format (e.g., vLLM, Groq, Together AI).

---
*Unifying the world of LLMs into a single C# interface.*

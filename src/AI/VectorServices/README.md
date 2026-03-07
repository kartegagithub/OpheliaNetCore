# Ophelia AI Vector Services

Implementations for storing and retrieving multidimensional vectors, enabling similarity search and long-term memory for AI agents.

## 📂 Supported Vector Databases

| Service | Provider | Description |
| :--- | :--- | :--- |
| **[ElasticSearchService.cs](./ElasticSearchService.cs)** | ElasticSearch | Enterprise-grade vector storage and full-text search. |
| **[PineconeService.cs](./PineconeService.cs)** | Pinecone | Managed cloud vector database optimized for speed. |
| **[RedisService.cs](./RedisService.cs)** | Redis | Ultra-fast, in-memory vector search using Redis-Stack. |

## 🛠 Key Capabilities
- **Vector Indexing**: Storing text embeddings for fast retrieval.
- **Similarity Search**: Finding the most relevant documents based on cosine similarity or Euclidean distance.
- **Metadata Filtering**: Combining vector search with traditional relational-style filters.

---
*Bridging the gap between unstructured data and intent.*

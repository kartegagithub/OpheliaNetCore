# Ophelia Cryptography

This module provides essential tools for securing sensitive data through encryption, hashing, and encoding.

## 📂 Security Tools

### [CryptoManager.cs](./CryptoManager.cs)
The main service for encryption and decryption. It supports symmetric algorithms and provides easy-to-use methods for securing strings and byte arrays.
- **Key Methods**: `Encrypt`, `Decrypt`, `GenerateKey`.

### [CryptoHelper.cs](./CryptoHelper.cs)
A collection of static helpers for quick operations like MD5/SHA hashing and Base64 encoding/decoding.
- **Key Methods**: `ToMD5`, `ToSHA256`, `Base64Encode`, `Base64Decode`.

### [Algorithms.cs](./Algorithms.cs)
Defines the encryption standards and settings used across the framework (e.g., AES settings).

## 🛡 Security Best Practices
- Always store encryption keys in a secure configuration or vault.
- Use `SHA256` or higher for sensitive data hashing.
- Encrypt PII (Personally Identifiable Information) before persisting to the database.

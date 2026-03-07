# Ophelia Integration Amazon

This module provides integration with Amazon Web Services (AWS), focusing primarily on cloud storage via S3.

## 📂 Core Services

### [AmazonS3Service.cs](./AmazonS3Service.cs)
A comprehensive service for interacting with AWS S3 buckets. It handles file uploads, downloads, deletions, and bucket management.
- **Key Methods**: `UploadFileAsync`, `GetFileAsync`, `DeleteFileAsync`, `ListFilesAsync`, `GeneratePreSignedUrl`.

## 📁 Subdirectories
- **[Model](./Model)**: Contains request and response models specific to AWS operations (e.g., `S3ObjectInfo`).

## 🚀 Configuration

To use the Amazon service, ensure you have your AWS credentials (Access Key and Secret Key) properly configured.

```csharp
var s3Service = new AmazonS3Service(accessKey, secretKey, region);
await s3Service.UploadFileAsync("my-bucket", "folder/image.png", fileBytes);
```

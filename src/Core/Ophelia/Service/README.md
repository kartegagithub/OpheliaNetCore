# Ophelia Service Layer

The Service module defines the standard patterns for API responses, request handling, and exception management within the Ophelia Framework.

## 📂 Core Response Models

### [ServiceResult.cs](./ServiceResult.cs)
The base class for all internal and external service responses. It encapsulates data, success status, and a collection of messages.
- **Key Properties**: `Success`, `Data`, `Messages`, `ExecutionTime`.

### [ServiceCollectionResult.cs](./ServiceCollectionResult.cs)
A specialized result for returning collections, including pagination metadata.

## 📂 Infrastructure & Monitoring

### [ServiceLogHandler.cs](./ServiceLogHandler.cs)
Manages the logging of service inputs and outputs, vital for audit trails and debugging complex microservice interactions.

### [ServicePerformance.cs](./ServicePerformance.cs)
Tracks execution times and performance metrics for individual service calls.

### [ServiceExceptionHandler.cs](./ServiceExceptionHandler.cs)
A global handler that ensures all exceptions are caught and transformed into a standard `ServiceResult` with appropriate error messages.

## 📁 WCF / SOAP Interceptors
- **[LoggingEndpointBehaviour.cs](./LoggingEndpointBehaviour.cs)** & **[LoggingMessageInspector.cs](./LoggingMessageInspector.cs)**: Tools for extending and logging legacy SOAP services.

---
*Providing a consistent contract for every operation.*

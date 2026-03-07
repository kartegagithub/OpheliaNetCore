# Ophelia Data Logging & Auditing

Tools for tracking database operations and changes to entity data.

## 📂 Components

### [AuditLog.cs](./AuditLog.cs)
Represents a single entry in the audit trail, capturing who changed what and when.

### [IAuditLogger.cs](./IAuditLogger.cs)
The interface for implementing custom audit logging strategies.

### [AuditLoggingAttribute.cs](./AuditLoggingAttribute.cs)
A marker attribute to enable automatic auditing for specific entity classes.

---
*Maintaining a reliable history of data changes.*

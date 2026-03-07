# Ophelia Tasks

The Tasks module provides a robust framework for managing background jobs, scheduled routines, and automated processes.

## 📂 Job Management

### [JobManager.cs](./JobManager.cs)
The orchestrator for all background activities. It handles job registration, scheduling, and monitoring.
- **Key Methods**: `RegisterJob`, `RunNow`, `StopAll`, `GetJobStatus`.

### [Job.cs](./Job.cs)
The base class for all background tasks. Developers should inherit from this class to implement custom logic.
- **Key Properties**: `Interval`, `IsRunning`, `LastRunDate`.

### [Routine.cs](./Routine.cs)
Specialized job implementation for periodic maintenance or synchronization tasks.

### [LogHandler.cs](./LogHandler.cs)
Provides specialized logging for task execution, helping in auditing and troubleshooting background processes.

## 🛠 Attrubutes
- **[JobMethodAttribute.cs](./JobMethodAttribute.cs)**: Marks a method as a target for automated execution by the JobManager.

---
*Built for scale and reliability in background processing.*

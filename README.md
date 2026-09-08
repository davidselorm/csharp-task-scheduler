# csharp-task-scheduler

High-performance work-stealing task scheduler with dynamic load balancing in C# and .NET 8.

## Architecture
- **Work-Stealing Queues**: Per-worker-thread task queues reducing lock contention on SMP hardware.
- **Cooperative Work Stealing**: Worker threads steal unexecuted work units from neighbor queues when idle.

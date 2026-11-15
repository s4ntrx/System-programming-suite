# System Programming Suite

A single Windows Forms project (.NET 8, C#) with a main menu that opens five modules. Each module calls real operating-system APIs through the .NET runtime or P/Invoke.

| Module | Concepts demonstrated | Key APIs |
|---|---|---|
| Process Management | Enumerate, create, terminate, suspend, resume | `Process`, `OpenProcess`, `NtSuspendProcess`, `NtResumeProcess` |
| Threads and Synchronization | Race conditions, locks, mutex, semaphore, cancellation, parallelism | `Thread`, `lock`, `Interlocked`, `Mutex`, `SemaphoreSlim`, `Parallel.For` |
| File System and I/O | CRUD, attributes, exclusive locks, change notification | `File`, `FileStream`, `FileShare.None`, `FileSystemWatcher`, `DriveInfo` |
| Memory Management | Virtual memory, heap, GC, shared memory | `VirtualAlloc`, `VirtualFree`, `GC`, `MemoryMappedFile` |
| Inter-Process Communication | Named pipes, stdout redirection | `NamedPipeServerStream`, `NamedPipeClientStream`, `ProcessStartInfo.RedirectStandardOutput` |

## Build and run

Requires Windows 10/11 and the .NET 8 SDK (or Visual Studio 2022 with the ".NET desktop development" workload).

    dotnet run

In Visual Studio: open `SystemProgrammingSuite.csproj` and press F5.

## Notes

- Stopping or suspending system processes may require running as administrator. The app refuses to stop or suspend itself.
- Pausing a process freezes it completely. Resume it before closing the app.
- File demos only touch `%TEMP%\SysProgSuite`.

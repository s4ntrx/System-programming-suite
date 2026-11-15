# System Programming Suite

A Windows Forms app (C#, .NET 8) with a main menu and five modules. Every button calls a real operating-system feature.


| Module | Buttons | Concept |
|---|---|---|
| Process Management | List, Start, Stop, Pause, Resume | Processes, Windows API calls |
| Threads and Synchronization | Start thread, Race (no lock), Race (lock), Semaphore | Threads, race conditions, locks |
| File System | Create, Append, Read, Info, Delete | File I/O |
| Memory Management | Info, Allocate, Write/Read, Free, GC | Virtual memory, garbage collection |
| Inter-Process Communication | Start server, Send, Stop server, Child process | Named pipes, process output |

## Run

Needs Windows 10/11 and the .NET 8 SDK.

    dotnet --list-sdks
    dotnet run

Or open the `.csproj` in Visual Studio 2022 and press F5.

## Notes

- Pause, Resume and Stop on other programs may need "Run as administrator".
- Files are stored in `%TEMP%\SysProgSuite`.

# Requirements
    VS code
    C# Dev Kit - vs code extensoin
    .NET 8 SDK - from https://dotnet.microsoft.com/download/dotnet/8.0

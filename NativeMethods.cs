using System.Runtime.InteropServices;

namespace SystemProgrammingSuite;

// Direct calls into Windows DLLs (P/Invoke). Used by the Process and Memory modules.
internal static class NativeMethods
{
    public const uint ProcessSuspendResume = 0x0800;
    public const uint MemCommitReserve = 0x3000;
    public const uint MemRelease = 0x8000;
    public const uint PageReadWrite = 0x04;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint access, bool inherit, int processId);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("ntdll.dll")]
    public static extern int NtSuspendProcess(IntPtr handle);

    [DllImport("ntdll.dll")]
    public static extern int NtResumeProcess(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAlloc(IntPtr address, UIntPtr size, uint type, uint protect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualFree(IntPtr address, UIntPtr size, uint type);
}

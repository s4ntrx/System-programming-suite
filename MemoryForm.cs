using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SystemProgrammingSuite.NativeMethods;

namespace SystemProgrammingSuite;

public sealed class MemoryForm : ModuleForm
{
    private const int BlockSize = 16 * 1024 * 1024;
    private IntPtr block = IntPtr.Zero;

    public MemoryForm() : base("Memory Management")
    {
        AddButton("Memory info", ShowInfo);
        AddButton("Allocate 16 MB", Allocate);
        AddButton("Write and read", WriteAndRead);
        AddButton("Free memory", Free);
        AddButton("Managed memory and GC", GarbageCollect);
    }

    private void ShowInfo()
    {
        var p = Process.GetCurrentProcess();
        Write($"Working set: {p.WorkingSet64 / 1048576.0:F1} MB. Page size: {Environment.SystemPageSize} bytes.");
    }

    // Ask Windows directly for memory.
    private void Allocate()
    {
        if (block != IntPtr.Zero) { Write("Already allocated. Free it first."); return; }
        block = VirtualAlloc(IntPtr.Zero, (UIntPtr)BlockSize, MemCommitReserve, PageReadWrite);
        if (block == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        Write($"Windows gave us 16 MB at address 0x{block.ToInt64():X}.");
    }

    private void WriteAndRead()
    {
        if (block == IntPtr.Zero) { Write("Allocate memory first."); return; }
        Marshal.WriteInt64(block, 0, 123456789L);
        Write($"Wrote 123456789, read back {Marshal.ReadInt64(block, 0)}.");
    }

    private void Free()
    {
        if (block == IntPtr.Zero) { Write("Nothing to free."); return; }
        VirtualFree(block, UIntPtr.Zero, MemRelease);
        block = IntPtr.Zero;
        Write("Memory returned to Windows.");
    }

    // .NET memory is cleaned up by the garbage collector, not by you.
    private void GarbageCollect()
    {
        byte[] data = new byte[100 * 1024 * 1024];
        data[0] = 1;
        Write($"After allocating 100 MB: heap is {GC.GetTotalMemory(false) / 1048576} MB.");
        data = null;
        GC.Collect();
        Write($"After GC.Collect: heap is {GC.GetTotalMemory(true) / 1048576} MB.");
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (block != IntPtr.Zero) VirtualFree(block, UIntPtr.Zero, MemRelease);
        base.OnFormClosed(e);
    }
}

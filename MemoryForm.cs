using System.ComponentModel;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using static SystemProgrammingSuite.NativeMethods;

namespace SystemProgrammingSuite;

public sealed class MemoryForm : ModuleForm
{
    private const int NativeBlockSize = 16 * 1024 * 1024;
    private const int ManagedBlockSize = 100 * 1024 * 1024;
    private const string MapName = "SysProgSuiteMap";

    private readonly List<IntPtr> nativeBlocks = new();
    private readonly List<byte[]> managedBlocks = new();

    public MemoryForm() : base("Module 4: Memory Management")
    {
        AddButton("Memory stats", ShowStats);
        AddButton("VirtualAlloc 16 MB", AllocateNative);
        AddButton("Write and verify", WriteAndVerify);
        AddButton("VirtualFree all", FreeNative);
        AddButton("Allocate managed 100 MB", AllocateManaged);
        AddButton("Release and GC", ReleaseManaged);
        AddButton("Memory-mapped file", MappedFileDemo);
    }

    private void ShowStats()
    {
        using var process = Process.GetCurrentProcess();
        var gc = GC.GetGCMemoryInfo();
        Write($"Working set: {process.WorkingSet64 / 1048576.0:F1} MB, private bytes: {process.PrivateMemorySize64 / 1048576.0:F1} MB");
        Write($"Managed heap: {GC.GetTotalMemory(false) / 1048576.0:F1} MB, collections gen0/1/2: {GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)}");
        Write($"Page size: {Environment.SystemPageSize} bytes, physical RAM available to GC: {gc.TotalAvailableMemoryBytes / 1073741824.0:F1} GB");
    }

    private void AllocateNative()
    {
        IntPtr block = VirtualAlloc(IntPtr.Zero, (UIntPtr)NativeBlockSize, MemCommit | MemReserve, PageReadWrite);
        if (block == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        nativeBlocks.Add(block);
        Write($"VirtualAlloc returned 16 MB at 0x{block.ToInt64():X}. Blocks held: {nativeBlocks.Count}");
    }

    private void WriteAndVerify()
    {
        if (nativeBlocks.Count == 0) { Write("Allocate a native block first."); return; }
        IntPtr block = nativeBlocks[^1];
        const long pattern = 0x1122334455667788L;
        Marshal.WriteInt64(block, 0, pattern);
        Marshal.WriteInt64(block, NativeBlockSize - 8, pattern);
        bool ok = Marshal.ReadInt64(block, 0) == pattern && Marshal.ReadInt64(block, NativeBlockSize - 8) == pattern;
        Write($"Wrote pattern at first and last 8 bytes of the block, read back matches: {ok}");
    }

    private void FreeNative()
    {
        foreach (IntPtr block in nativeBlocks) VirtualFree(block, UIntPtr.Zero, MemRelease);
        Write($"VirtualFree released {nativeBlocks.Count} block(s).");
        nativeBlocks.Clear();
    }

    private void AllocateManaged()
    {
        var block = new byte[ManagedBlockSize];
        for (int i = 0; i < block.Length; i += Environment.SystemPageSize) block[i] = 1;
        managedBlocks.Add(block);
        Write($"Allocated and touched 100 MB array. Managed heap now {GC.GetTotalMemory(false) / 1048576.0:F0} MB.");
    }

    private void ReleaseManaged()
    {
        long before = GC.GetTotalMemory(false);
        managedBlocks.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long after = GC.GetTotalMemory(true);
        Write($"Managed heap {before / 1048576.0:F0} MB to {after / 1048576.0:F0} MB after GC.Collect.");
    }

    private void MappedFileDemo()
    {
        using var map = MemoryMappedFile.CreateNew(MapName, 4096);
        byte[] message = Encoding.UTF8.GetBytes($"Shared at {DateTime.Now:T}");
        using (var writer = map.CreateViewAccessor())
        {
            writer.Write(0, message.Length);
            writer.WriteArray(4, message, 0, message.Length);
        }

        using var second = MemoryMappedFile.OpenExisting(MapName);
        using var reader = second.CreateViewAccessor();
        int length = reader.ReadInt32(0);
        var buffer = new byte[length];
        reader.ReadArray(4, buffer, 0, length);
        Write($"Second view of '{MapName}' read: \"{Encoding.UTF8.GetString(buffer)}\"");
        Write("Another process could open the same name and see this data.");
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (IntPtr block in nativeBlocks) VirtualFree(block, UIntPtr.Zero, MemRelease);
        nativeBlocks.Clear();
        base.OnFormClosed(e);
    }
}

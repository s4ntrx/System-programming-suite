using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SystemProgrammingSuite.NativeMethods;

namespace SystemProgrammingSuite;

public sealed class ProcessForm : ModuleForm
{
    private readonly ListView list = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };

    public ProcessForm() : base("Process Management")
    {
        list.Columns.Add("PID", 80);
        list.Columns.Add("Name", 260);
        list.Columns.Add("Memory (MB)", 110);
        Body.Controls.Add(list);

        AddButton("List processes", ListProcesses);
        AddButton("Start Notepad", StartNotepad);
        AddButton("Stop selected", StopSelected);
        AddButton("Pause selected", () => Suspend(true));
        AddButton("Resume selected", () => Suspend(false));

        Shown += (_, _) => ListProcesses();
    }

    // Returns the PID of the selected row, or -1 if nothing is selected.
    private int SelectedPid()
    {
        if (list.SelectedItems.Count == 0) { Write("Select a process first."); return -1; }
        int pid = int.Parse(list.SelectedItems[0].Text);
        if (pid == Environment.ProcessId) { Write("That is this application. Pick another."); return -1; }
        return pid;
    }

    private void ListProcesses()
    {
        list.Items.Clear();
        foreach (var p in Process.GetProcesses().OrderBy(p => p.ProcessName))
            list.Items.Add(new ListViewItem(new[] { p.Id.ToString(), p.ProcessName, (p.WorkingSet64 / 1048576.0).ToString("F1") }));
        Write($"Found {list.Items.Count} processes.");
    }

    private void StartNotepad()
    {
        var p = Process.Start("notepad.exe");
        Write($"Started Notepad, PID {p.Id}.");
        ListProcesses();
    }

    private void StopSelected()
    {
        int pid = SelectedPid();
        if (pid < 0) return;
        var p = Process.GetProcessById(pid);
        p.Kill();
        p.WaitForExit(3000);
        Write($"Stopped PID {pid}.");
        ListProcesses();
    }

    // Pausing needs the Windows API: open the process, then suspend or resume it.
    private void Suspend(bool pause)
    {
        int pid = SelectedPid();
        if (pid < 0) return;
        IntPtr handle = OpenProcess(ProcessSuspendResume, false, pid);
        if (handle == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        if (pause) NtSuspendProcess(handle); else NtResumeProcess(handle);
        CloseHandle(handle);
        Write($"{(pause ? "Paused" : "Resumed")} PID {pid}.");
    }
}

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static SystemProgrammingSuite.NativeMethods;

namespace SystemProgrammingSuite;

public sealed class ProcessForm : ModuleForm
{
    private readonly ListView list = new()
    {
        Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
        GridLines = true, MultiSelect = false, HideSelection = false
    };
    private readonly TextBox programBox = new() { Text = "notepad.exe", Width = 200 };

    public ProcessForm() : base("Module 1: Process Management")
    {
        list.Columns.Add("PID", 70);
        list.Columns.Add("Name", 240);
        list.Columns.Add("Memory (MB)", 110);
        list.Columns.Add("Threads", 80);
        list.Columns.Add("Priority", 110);
        Body.Controls.Add(list);

        AddLabel("Program:");
        Toolbar.Controls.Add(programBox);
        AddButton("Start", StartProcess);
        AddButton("Stop", StopProcess);
        AddButton("Pause", () => SetSuspended(true));
        AddButton("Resume", () => SetSuspended(false));
        AddButton("Details", ShowDetails);
        AddButton("Refresh list", RefreshList);

        Shown += (_, _) => RefreshList();
    }

    private int? SelectedPid()
    {
        if (list.SelectedItems.Count == 0)
        {
            Write("Select a process in the list first.");
            return null;
        }
        return int.Parse(list.SelectedItems[0].Text);
    }

    private void RefreshList()
    {
        list.BeginUpdate();
        list.Items.Clear();
        foreach (var process in Process.GetProcesses().OrderBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase))
        {
            using (process)
            {
                string priority = "n/a";
                int threads = 0;
                try { priority = process.PriorityClass.ToString(); } catch { }
                try { threads = process.Threads.Count; } catch { }
                list.Items.Add(new ListViewItem(new[]
                {
                    process.Id.ToString(), process.ProcessName,
                    (process.WorkingSet64 / 1048576.0).ToString("F1"), threads.ToString(), priority
                }));
            }
        }
        list.EndUpdate();
        Write($"Listed {list.Items.Count} processes.");
    }

    private void StartProcess()
    {
        string path = programBox.Text.Trim();
        using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        Write($"Started '{path}' with PID {process?.Id}.");
        RefreshList();
    }

    private void StopProcess()
    {
        if (SelectedPid() is not int pid) return;
        if (pid == Environment.ProcessId) { Write("Refusing to terminate this application."); return; }

        using var process = Process.GetProcessById(pid);
        var answer = MessageBox.Show($"Terminate {process.ProcessName} (PID {pid})?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes) return;

        process.Kill();
        bool exited = process.WaitForExit(3000);
        Write(exited ? $"Process {pid} terminated." : $"Kill signal sent to {pid}, still shutting down.");
        RefreshList();
    }

    private void SetSuspended(bool suspend)
    {
        if (SelectedPid() is not int pid) return;
        if (pid == Environment.ProcessId) { Write("Refusing to suspend this application."); return; }

        IntPtr handle = OpenProcess(ProcessSuspendResume, false, pid);
        if (handle == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            int status = suspend ? NtSuspendProcess(handle) : NtResumeProcess(handle);
            Write($"{(suspend ? "Suspended" : "Resumed")} PID {pid} (NTSTATUS 0x{status:X8}).");
        }
        finally { CloseHandle(handle); }
    }

    private void ShowDetails()
    {
        if (SelectedPid() is not int pid) return;
        using var process = Process.GetProcessById(pid);
        Write($"--- {process.ProcessName} (PID {pid}) ---");
        try
        {
            Write($"Started: {process.StartTime}");
            Write($"CPU time: {process.TotalProcessorTime}");
            Write($"Handles: {process.HandleCount}, threads: {process.Threads.Count}");
            Write($"Image: {process.MainModule?.FileName}");
        }
        catch (Win32Exception) { Write("Some fields need administrator rights for this process."); }
        catch (InvalidOperationException) { Write("The process has already exited."); }
    }
}

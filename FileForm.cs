using System.Text;

namespace SystemProgrammingSuite;

public sealed class FileForm : ModuleForm
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "SysProgSuite");
    private readonly TextBox editor = new()
    {
        Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 10f), Text = "Text written to demo.txt by the File System module."
    };
    private FileSystemWatcher watcher;

    private string FilePath => Path.Combine(directory, "demo.txt");

    public FileForm() : base("Module 3: File System and I/O")
    {
        Body.Controls.Add(editor);
        AddButton("Create / overwrite", CreateFile);
        AddButton("Append line", AppendLine);
        AddButton("Read into editor", ReadFile);
        AddButton("Copy", CopyFile);
        AddButton("File info", ShowInfo);
        AddButton("Toggle read-only", ToggleReadOnly);
        AddButton("Exclusive lock demo", LockDemo);
        AddButton("List folder", ListFolder);
        AddButton("Start watcher", StartWatcher);
        AddButton("Stop watcher", StopWatcher);
        AddButton("Drives", ShowDrives);
        AddButton("Delete all", DeleteAll);
        Write($"Working folder: {directory}");
    }

    private void CreateFile()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(FilePath, editor.Text, Encoding.UTF8);
        Write($"Wrote {new FileInfo(FilePath).Length} bytes to demo.txt.");
    }

    private void AppendLine()
    {
        Directory.CreateDirectory(directory);
        File.AppendAllText(FilePath, $"{Environment.NewLine}Appended at {DateTime.Now:T}");
        Write("Appended a line.");
    }

    private void ReadFile()
    {
        editor.Text = File.ReadAllText(FilePath);
        Write("Loaded demo.txt into the editor.");
    }

    private void CopyFile()
    {
        string target = Path.Combine(directory, "demo_copy.txt");
        File.Copy(FilePath, target, true);
        Write($"Copied to {Path.GetFileName(target)}.");
    }

    private void ShowInfo()
    {
        var info = new FileInfo(FilePath);
        if (!info.Exists) { Write("demo.txt does not exist yet."); return; }
        Write($"Size: {info.Length} bytes, attributes: {info.Attributes}");
        Write($"Created: {info.CreationTime}, modified: {info.LastWriteTime}");
    }

    private void ToggleReadOnly()
    {
        var info = new FileInfo(FilePath) { };
        info.IsReadOnly = !info.IsReadOnly;
        Write($"Read-only is now {info.IsReadOnly}.");
    }

    private void LockDemo()
    {
        Directory.CreateDirectory(directory);
        using var locked = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        Write("Opened demo.txt with FileShare.None. Trying a second open...");
        try { File.ReadAllText(FilePath); }
        catch (IOException ex) { Write($"Second open failed as expected: {ex.Message}"); }
        Write("Lock released when the first stream closes.");
    }

    private void ListFolder()
    {
        if (!Directory.Exists(directory)) { Write("Folder does not exist yet."); return; }
        foreach (var file in new DirectoryInfo(directory).GetFiles())
            Write($"{file.Name,-20} {file.Length,8} bytes  {file.Attributes}");
        Write("Listing complete.");
    }

    private void StartWatcher()
    {
        if (watcher != null) { Write("Watcher is already running."); return; }
        Directory.CreateDirectory(directory);
        watcher = new FileSystemWatcher(directory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        FileSystemEventHandler report = (_, e) => Write($"Watcher: {e.ChangeType} {e.Name}");
        watcher.Created += report;
        watcher.Changed += report;
        watcher.Deleted += report;
        watcher.Renamed += (_, e) => Write($"Watcher: Renamed {e.OldName} to {e.Name}");
        watcher.EnableRaisingEvents = true;
        Write("Watching the folder. Use the other buttons to trigger events.");
    }

    private void StopWatcher()
    {
        watcher?.Dispose();
        watcher = null;
        Write("Watcher stopped.");
    }

    private void ShowDrives()
    {
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            Write($"{drive.Name} {drive.DriveFormat}, {drive.TotalFreeSpace / 1073741824.0:F1} GB free of {drive.TotalSize / 1073741824.0:F1} GB");
    }

    private void DeleteAll()
    {
        if (!Directory.Exists(directory)) { Write("Nothing to delete."); return; }
        foreach (var file in new DirectoryInfo(directory).GetFiles())
        {
            file.IsReadOnly = false;
            file.Delete();
        }
        Write("Deleted all demo files.");
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        watcher?.Dispose();
        base.OnFormClosed(e);
    }
}

namespace SystemProgrammingSuite;

public sealed class FileForm : ModuleForm
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "SysProgSuite");
    private readonly string file;
    private readonly TextBox editor = new() { Dock = DockStyle.Fill, Multiline = true, Text = "Hello from the File System module." };

    public FileForm() : base("File System")
    {
        file = Path.Combine(folder, "demo.txt");
        Body.Controls.Add(editor);

        AddButton("Create file", CreateFile);
        AddButton("Append line", AppendLine);
        AddButton("Read file", ReadFile);
        AddButton("File info", ShowInfo);
        AddButton("Delete file", DeleteFile);
        Write($"Files are stored in {folder}");
    }

    private void CreateFile()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(file, editor.Text);
        Write("File created from the text box.");
    }

    private void AppendLine()
    {
        File.AppendAllText(file, Environment.NewLine + "Added at " + DateTime.Now.ToString("T"));
        Write("Line appended.");
    }

    private void ReadFile()
    {
        editor.Text = File.ReadAllText(file);
        Write("File loaded into the text box.");
    }

    private void ShowInfo()
    {
        var info = new FileInfo(file);
        if (!info.Exists) { Write("File does not exist yet."); return; }
        Write($"Size: {info.Length} bytes. Created: {info.CreationTime}. Modified: {info.LastWriteTime}.");
    }

    private void DeleteFile()
    {
        File.Delete(file);
        Write("File deleted.");
    }
}

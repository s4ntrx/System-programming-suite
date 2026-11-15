using System.Diagnostics;

namespace SystemProgrammingSuite;

public sealed class MainForm : Form
{
    private static readonly Color Paper = Color.FromArgb(0xF4, 0xF1, 0xEA);
    private static readonly Color Ink = Color.FromArgb(0x1F, 0x29, 0x33);
    private static readonly Color Rust = Color.FromArgb(0xB4, 0x50, 0x1E);

    private sealed record ModuleInfo(string Title, string Description, Func<Form> Create);

    private readonly ModuleInfo[] modules =
    {
        new("Process Management", "List, start, stop, pause and resume operating-system processes.", () => new ProcessForm()),
        new("Threads and Synchronization", "Race conditions, locks, mutexes, semaphores, cancellation and parallel loops.", () => new ThreadForm()),
        new("File System and I/O", "Create, read, copy and delete files, exclusive locks and live directory watching.", () => new FileForm()),
        new("Memory Management", "VirtualAlloc, managed heap, garbage collection and memory-mapped files.", () => new MemoryForm()),
        new("Inter-Process Communication", "Named pipe server and client, plus capturing a child process's output.", () => new IpcForm()),
    };

    public MainForm()
    {
        Text = "System Programming Suite";
        ClientSize = new Size(780, 580);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Paper;
        Font = new Font("Segoe UI", 10f);

        Controls.Add(new Label
        {
            Text = "System Programming Suite", Font = new Font("Georgia", 22f, FontStyle.Bold),
            ForeColor = Ink, AutoSize = true, Location = new Point(32, 24)
        });
        Controls.Add(new Label
        {
            Text = $"Running as PID {Environment.ProcessId} on {Environment.OSVersion.VersionString}, {Environment.ProcessorCount} logical processors.",
            ForeColor = Color.DimGray, AutoSize = true, Location = new Point(35, 76)
        });

        for (int i = 0; i < modules.Length; i++)
        {
            var module = modules[i];
            int y = 120 + i * 78;
            var open = new Button
            {
                Text = $"{i + 1}   {module.Title}", Size = new Size(290, 58), Location = new Point(32, y),
                FlatStyle = FlatStyle.Flat, BackColor = Ink, ForeColor = Paper,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand
            };
            open.FlatAppearance.BorderSize = 0;
            open.FlatAppearance.MouseOverBackColor = Rust;
            open.Click += (_, _) => module.Create().Show(this);
            Controls.Add(open);
            Controls.Add(new Label
            {
                Text = module.Description, Location = new Point(340, y + 6), Size = new Size(400, 48), ForeColor = Ink
            });
        }

        var exit = new Button
        {
            Text = "Exit", Size = new Size(110, 34), Location = new Point(32, 528),
            FlatStyle = FlatStyle.Flat, ForeColor = Ink, Cursor = Cursors.Hand
        };
        exit.Click += (_, _) => Close();
        Controls.Add(exit);
    }
}

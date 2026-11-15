namespace SystemProgrammingSuite;

/// <summary>Shared layout for every module: toolbar on top, content in the middle, log at the bottom.</summary>
public abstract class ModuleForm : Form
{
    private static readonly Color Paper = Color.FromArgb(0xF4, 0xF1, 0xEA);
    private static readonly Color Sand = Color.FromArgb(0xE8, 0xE3, 0xD8);
    private static readonly Color Ink = Color.FromArgb(0x1F, 0x29, 0x33);
    private static readonly Color Rust = Color.FromArgb(0xB4, 0x50, 0x1E);

    protected readonly FlowLayoutPanel Toolbar = new()
    {
        Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Padding = new Padding(8), BackColor = Sand
    };

    protected readonly Panel Body = new() { Dock = DockStyle.Fill, Padding = new Padding(8) };

    private readonly TextBox logBox = new()
    {
        Dock = DockStyle.Bottom, Height = 190, Multiline = true, ReadOnly = true,
        ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9.5f),
        BackColor = Ink, ForeColor = Sand
    };

    protected ModuleForm(string title)
    {
        Text = title;
        ClientSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Paper;
        Controls.Add(Body);
        Controls.Add(logBox);
        Controls.Add(Toolbar);
    }

    protected Button AddButton(string text, Action action)
    {
        var button = CreateButton(text);
        button.Click += (_, _) =>
        {
            try { action(); }
            catch (Exception ex) { Write($"ERROR ({ex.GetType().Name}): {ex.Message}"); }
        };
        return button;
    }

    protected Button AddAsyncButton(string text, Func<Task> action)
    {
        var button = CreateButton(text);
        button.Click += async (_, _) =>
        {
            try { await action(); }
            catch (Exception ex) { Write($"ERROR ({ex.GetType().Name}): {ex.Message}"); }
        };
        return button;
    }

    private Button CreateButton(string text)
    {
        var button = new Button
        {
            Text = text, AutoSize = true, Padding = new Padding(6, 2, 6, 2),
            FlatStyle = FlatStyle.Flat, BackColor = Ink, ForeColor = Paper, Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Rust;
        Toolbar.Controls.Add(button);
        return button;
    }

    protected void AddLabel(string text) =>
        Toolbar.Controls.Add(new Label { Text = text, AutoSize = true, Margin = new Padding(8, 8, 0, 0) });

    /// <summary>Thread-safe logging; background threads can call this directly.</summary>
    protected void Write(string message)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            try { BeginInvoke(new Action(() => Write(message))); }
            catch (InvalidOperationException) { }
            return;
        }
        logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}

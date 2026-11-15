namespace SystemProgrammingSuite;

// Base class for all five modules: button bar on top, log at the bottom.
public abstract class ModuleForm : Form
{
    protected readonly FlowLayoutPanel Buttons = new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
    protected readonly Panel Body = new() { Dock = DockStyle.Fill };
    private readonly TextBox log = new()
    {
        Dock = DockStyle.Bottom, Height = 180, Multiline = true, ReadOnly = true,
        ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 10)
    };

    protected ModuleForm(string title)
    {
        Text = title;
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;
        Controls.Add(Body);
        Controls.Add(log);
        Controls.Add(Buttons);
    }

    protected void AddButton(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += (_, _) =>
        {
            try { action(); }
            catch (Exception ex) { Write("ERROR: " + ex.Message); }
        };
        Buttons.Controls.Add(button);
    }

    // Safe to call from any thread.
    protected void Write(string message)
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            try { BeginInvoke(new Action(() => Write(message))); } catch (InvalidOperationException) { }
            return;
        }
        log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}

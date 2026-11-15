namespace SystemProgrammingSuite;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "System Programming Suite";
        ClientSize = new Size(380, 340);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        Controls.Add(new Label { Text = "Choose a module", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) });

        AddMenuButton("1. Process Management", 0, () => new ProcessForm());
        AddMenuButton("2. Threads and Synchronization", 1, () => new ThreadForm());
        AddMenuButton("3. File System", 2, () => new FileForm());
        AddMenuButton("4. Memory Management", 3, () => new MemoryForm());
        AddMenuButton("5. Inter-Process Communication", 4, () => new IpcForm());

        var exit = new Button { Text = "Exit", Size = new Size(340, 32), Location = new Point(20, 295) };
        exit.Click += (_, _) => Close();
        Controls.Add(exit);
    }

    private void AddMenuButton(string text, int row, Func<Form> createForm)
    {
        var button = new Button { Text = text, Size = new Size(340, 40), Location = new Point(20, 55 + row * 48) };
        button.Click += (_, _) => createForm().Show(this);
        Controls.Add(button);
    }
}

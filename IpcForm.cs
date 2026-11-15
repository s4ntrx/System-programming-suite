using System.Diagnostics;
using System.IO.Pipes;

namespace SystemProgrammingSuite;

public sealed class IpcForm : ModuleForm
{
    private const string PipeName = "SysProgSuitePipe";
    private readonly TextBox messageBox = new() { Text = "hello", Width = 200 };
    private CancellationTokenSource serverStop;

    public IpcForm() : base("Inter-Process Communication")
    {
        Buttons.Controls.Add(new Label { Text = "Message:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
        Buttons.Controls.Add(messageBox);
        AddButton("Start server", StartServer);
        AddButton("Send message", SendMessage);
        AddButton("Stop server", StopServer);
        AddButton("Run child process", RunChild);
    }

    private void StartServer()
    {
        if (serverStop != null) { Write("Server already running."); return; }
        serverStop = new CancellationTokenSource();
        _ = ServerLoop(serverStop.Token);
    }

    // Waits for a client, reads one line, replies, then waits for the next client.
    private async Task ServerLoop(CancellationToken token)
    {
        Write("Server is waiting for clients.");
        try
        {
            while (!token.IsCancellationRequested)
            {
                using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await pipe.WaitForConnectionAsync(token);
                using var reader = new StreamReader(pipe);
                using var writer = new StreamWriter(pipe) { AutoFlush = true };
                string line = await reader.ReadLineAsync();
                Write($"Server received: {line}");
                await writer.WriteLineAsync("Server says: " + line?.ToUpper());
                pipe.WaitForPipeDrain();
            }
        }
        catch (OperationCanceledException) { }
        Write("Server stopped.");
    }

    private void SendMessage()
    {
        using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
        pipe.Connect(2000);
        using var writer = new StreamWriter(pipe) { AutoFlush = true };
        using var reader = new StreamReader(pipe);
        writer.WriteLine(messageBox.Text);
        Write("Client got reply: " + reader.ReadLine());
    }

    private void StopServer()
    {
        if (serverStop == null) { Write("Server is not running."); return; }
        serverStop.Cancel();
        serverStop = null;
    }

    // Start another program and read what it prints through a pipe.
    private void RunChild()
    {
        var info = new ProcessStartInfo("cmd.exe", "/c echo Hello from the child process")
        {
            RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
        };
        using var child = Process.Start(info);
        string output = child.StandardOutput.ReadToEnd();
        child.WaitForExit();
        Write("Child said: " + output.Trim());
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        serverStop?.Cancel();
        base.OnFormClosed(e);
    }
}

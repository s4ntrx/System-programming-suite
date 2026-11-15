using System.Diagnostics;
using System.IO.Pipes;

namespace SystemProgrammingSuite;

public sealed class IpcForm : ModuleForm
{
    private const string PipeName = "SysProgSuitePipe";
    private readonly TextBox messageBox = new() { Text = "hello from the client", Width = 240 };
    private CancellationTokenSource serverCts;

    public IpcForm() : base("Module 5: Inter-Process Communication")
    {
        AddLabel("Message:");
        Toolbar.Controls.Add(messageBox);
        AddButton("Start pipe server", StartServer);
        AddAsyncButton("Send via client", SendMessageAsync);
        AddButton("Stop pipe server", StopServer);
        AddAsyncButton("Run child process", RunChildAsync);
        Write($"Pipe path: \\\\.\\pipe\\{PipeName}");
    }

    private void StartServer()
    {
        if (serverCts != null) { Write("Server is already running."); return; }
        serverCts = new CancellationTokenSource();
        _ = RunServerAsync(serverCts.Token);
    }

    private async Task RunServerAsync(CancellationToken token)
    {
        Write("Server: listening for clients.");
        try
        {
            while (!token.IsCancellationRequested)
            {
                await using var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(token);

                using var reader = new StreamReader(server);
                using var writer = new StreamWriter(server) { AutoFlush = true };
                string line = await reader.ReadLineAsync(token);
                Write($"Server: received \"{line}\"");
                await writer.WriteLineAsync($"ECHO ({line?.Length ?? 0} chars): {line?.ToUpperInvariant()}");
                server.WaitForPipeDrain();
            }
        }
        catch (OperationCanceledException) { }
        Write("Server: stopped.");
    }

    private async Task SendMessageAsync()
    {
        await using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
        using var writer = new StreamWriter(client) { AutoFlush = true };
        using var reader = new StreamReader(client);
        await writer.WriteLineAsync(messageBox.Text);
        Write($"Client: server replied \"{await reader.ReadLineAsync()}\"");
    }

    private void StopServer()
    {
        if (serverCts == null) { Write("Server is not running."); return; }
        serverCts.Cancel();
        serverCts = null;
    }

    private async Task RunChildAsync()
    {
        var info = new ProcessStartInfo("cmd.exe", "/c ver & echo Hello from the child process & echo User: %USERNAME%")
        {
            RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
        };
        using var child = Process.Start(info);
        string output = await child.StandardOutput.ReadToEndAsync();
        await child.WaitForExitAsync();
        Write($"Child PID {child.Id} exited with code {child.ExitCode}. Captured through its stdout pipe:");
        foreach (string line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            Write("  " + line);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        serverCts?.Cancel();
        base.OnFormClosed(e);
    }
}

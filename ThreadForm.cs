namespace SystemProgrammingSuite;

public sealed class ThreadForm : ModuleForm
{
    public ThreadForm() : base("Threads and Synchronization")
    {
        AddButton("Start a thread", StartThread);
        AddButton("Race condition (no lock)", () => Race(false));
        AddButton("Race fixed (with lock)", () => Race(true));
        AddButton("Semaphore demo", SemaphoreDemo);
    }

    // A second thread runs at the same time as the window.
    private void StartThread()
    {
        Write($"Window thread id: {Environment.CurrentManagedThreadId}");
        new Thread(() =>
        {
            for (int step = 1; step <= 5; step++)
            {
                Write($"Worker thread {Environment.CurrentManagedThreadId}, step {step}");
                Thread.Sleep(500);
            }
        }) { IsBackground = true }.Start();
    }

    // Four threads each add 1 to a shared counter 100,000 times. Correct answer: 400,000.
    private void Race(bool useLock)
    {
        Write(useLock ? "Running with lock..." : "Running without lock...");
        Task.Run(() =>
        {
            int counter = 0;
            object gate = new();
            var threads = new List<Thread>();
            for (int n = 0; n < 4; n++)
            {
                var worker = new Thread(() =>
                {
                    for (int i = 0; i < 100_000; i++)
                    {
                        if (useLock) { lock (gate) { counter++; } }
                        else counter++;
                    }
                });
                threads.Add(worker);
                worker.Start();
            }
            foreach (var worker in threads) worker.Join();
            Write($"Expected 400000, got {counter}");
        });
    }

    // Five workers, but only two may run at once.
    private void SemaphoreDemo()
    {
        var semaphore = new SemaphoreSlim(2);
        for (int n = 1; n <= 5; n++)
        {
            int id = n;
            new Thread(() =>
            {
                semaphore.Wait();
                Write($"Worker {id} entered");
                Thread.Sleep(1000);
                Write($"Worker {id} left");
                semaphore.Release();
            }) { IsBackground = true }.Start();
        }
    }
}

using System.Diagnostics;

namespace SystemProgrammingSuite;

public sealed class ThreadForm : ModuleForm
{
    private CancellationTokenSource workerCts;

    public ThreadForm() : base("Module 2: Threads and Synchronization")
    {
        AddButton("Thread info", ShowThreadInfo);
        AddButton("Race condition demo", RaceDemo);
        AddButton("Mutex demo", MutexDemo);
        AddButton("Semaphore demo", SemaphoreDemo);
        AddButton("Parallel vs sequential", ParallelDemo);
        AddButton("Start worker", StartWorker);
        AddButton("Stop worker", StopWorker);
    }

    private void ShowThreadInfo()
    {
        ThreadPool.GetAvailableThreads(out int workers, out int io);
        ThreadPool.GetMaxThreads(out int maxWorkers, out int maxIo);
        Write($"UI thread id: {Environment.CurrentManagedThreadId}, logical processors: {Environment.ProcessorCount}");
        Write($"Thread pool workers free: {workers}/{maxWorkers}, I/O free: {io}/{maxIo}");
        Write($"OS threads in this process: {Process.GetCurrentProcess().Threads.Count}");
    }

    private void RaceDemo()
    {
        Write("4 threads x 100,000 increments each, expected total 400,000...");
        Task.Run(() =>
        {
            const int threadCount = 4, iterations = 100_000;
            int unsafeCount = 0, lockedCount = 0, atomicCount = 0;
            var gate = new object();

            void RunOnThreads(Action step)
            {
                var threads = Enumerable.Range(0, threadCount)
                    .Select(_ => new Thread(() => { for (int i = 0; i < iterations; i++) step(); }))
                    .ToList();
                threads.ForEach(t => t.Start());
                threads.ForEach(t => t.Join());
            }

            RunOnThreads(() => unsafeCount++);
            RunOnThreads(() => { lock (gate) { lockedCount++; } });
            RunOnThreads(() => Interlocked.Increment(ref atomicCount));

            Write($"No synchronization: {unsafeCount:N0} (lost updates are the race condition)");
            Write($"lock statement:      {lockedCount:N0}");
            Write($"Interlocked:         {atomicCount:N0}");
        });
    }

    private void MutexDemo()
    {
        var mutex = new Mutex(false);
        var owner = new Thread(() =>
        {
            mutex.WaitOne();
            Write("Thread A: owns the mutex for 1.2 seconds.");
            Thread.Sleep(1200);
            mutex.ReleaseMutex();
            Write("Thread A: released the mutex.");
        }) { IsBackground = true };

        var contender = new Thread(() =>
        {
            Thread.Sleep(200);
            if (!mutex.WaitOne(400))
            {
                Write("Thread B: timed out, thread A still holds the lock. Waiting...");
                mutex.WaitOne();
            }
            Write("Thread B: acquired the mutex after A released it.");
            mutex.ReleaseMutex();
            mutex.Dispose();
        }) { IsBackground = true };

        owner.Start();
        contender.Start();
    }

    private void SemaphoreDemo()
    {
        var semaphore = new SemaphoreSlim(2);
        int active = 0;
        Write("5 workers compete for 2 semaphore slots.");
        for (int i = 1; i <= 5; i++)
        {
            int id = i;
            new Thread(() =>
            {
                semaphore.Wait();
                int now = Interlocked.Increment(ref active);
                Write($"Worker {id}: entered ({now} active)");
                Thread.Sleep(800);
                Interlocked.Decrement(ref active);
                Write($"Worker {id}: left");
                semaphore.Release();
            }) { IsBackground = true }.Start();
        }
    }

    private void ParallelDemo()
    {
        Write("Summing i % 7 over 50 million values...");
        Task.Run(() =>
        {
            const int count = 50_000_000;
            var timer = Stopwatch.StartNew();
            long sequential = 0;
            for (int i = 0; i < count; i++) sequential += i % 7;
            long sequentialMs = timer.ElapsedMilliseconds;

            timer.Restart();
            long parallel = 0;
            Parallel.For(0, count, () => 0L, (i, _, local) => local + i % 7, local => Interlocked.Add(ref parallel, local));
            long parallelMs = timer.ElapsedMilliseconds;

            Write($"Sequential: {sequentialMs} ms, Parallel.For: {parallelMs} ms, results match: {sequential == parallel}");
        });
    }

    private void StartWorker()
    {
        if (workerCts != null) { Write("Worker is already running."); return; }
        workerCts = new CancellationTokenSource();
        var token = workerCts.Token;
        new Thread(() =>
        {
            int tick = 0;
            while (!token.IsCancellationRequested)
            {
                Write($"Worker tick {++tick} on managed thread {Environment.CurrentManagedThreadId}");
                token.WaitHandle.WaitOne(700);
            }
            Write("Worker observed cancellation and exited.");
        }) { IsBackground = true, Name = "CancellableWorker" }.Start();
    }

    private void StopWorker()
    {
        if (workerCts == null) { Write("No worker is running."); return; }
        workerCts.Cancel();
        workerCts = null;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        workerCts?.Cancel();
        base.OnFormClosed(e);
    }
}

namespace Futures.Tests;

public class ThreadPoolExecutorTest
{
    [Fact]
    public void ShutdownShouldWakeUpThreads()
    {
        var executor = new ThreadPoolExecutor(4);
        var future = executor.Submit<string>(s => (string)s!, "foo");
        var result = future.GetResult();

        // Give a spawned thread some time to try to take the next element from the BlockingCollection
        Thread.Sleep(TimeSpan.FromMilliseconds(100));
        executor.Shutdown(wait: true);

        Assert.Equal("foo", result);
        Assert.Equal(1, executor.SpawnedThreads);
    }

    [Fact]
    public void ShouldComplete_N_Futures()
    {
        var executor = new ThreadPoolExecutor();
        static string callback(object? s)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            return (string)s!;
        }
        var futures = Enumerable.Range(1, 30)
            .Select(x => executor.Submit<string>(callback, $"task-{x}"))
            .ToList();
        var done = Future.Wait<string>(FutureWaitPolicy.AllCompleted, futures.ToArray());

        Assert.Equal(30, done.Count);
    }
}

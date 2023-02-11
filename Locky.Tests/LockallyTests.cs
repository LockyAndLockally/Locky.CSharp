using System.Collections.Concurrent;

namespace LockyAndLockally.Tests;

public class LockallyTests
{
    private static Lockally _test1Lockally = new();
    [Fact]
    public void Lock_locks()
    {
        _test1Lockally.Lock("LockallyTest1");
        Assert.False(_test1Lockally.TryLock("LockallyTest1"));
    }

    private static Lockally _test2Lockally = new();
    [Fact]
    public void TryLock_locks()
    {
        Assert.True(_test2Lockally.TryLock("LockallyTest2"));
        Assert.False(_test2Lockally.TryLock("LockallyTest2"));
        _test2Lockally.Release("LockallyTest2");
        Assert.True(_test2Lockally.TryLock("LockallyTest2"));
    }

    private static Lockally _test3Lockally1 = new();
    private static Lockally _test3Lockally2 = new();
    [Fact]
    public void Lockally_instances_are_independent_of_each_other()
    {
        Assert.True(_test3Lockally1.TryLock("LockallyTest3"));
        Assert.True(_test3Lockally2.TryLock("LockallyTest3"));
    }

    private static Lockally _test4Lockally = new();
    [Fact]
    public void Lockally_does_not_intefere_with_Locky()
    {
        Locky.Lock("LockallyTest4");
        Assert.True(_test4Lockally.TryLock("LockallyTest4"));
    }

    private static Lockally _test5Lockally = new();
    [Fact]
    public async Task Other_threads_work_the_same()
    {
        var lockName = "LockallyTest5";
        var tryLockResult = new ConcurrentQueue<bool>();
        _test5Lockally.Lock(lockName);
        var thread = new Thread(() => tryLockResult.Enqueue(_test5Lockally.TryLock(lockName)));
        thread.Start();
        await Task.Delay(50);
        Assert.False(Assert.Single(tryLockResult));
        Assert.False(_test5Lockally.TryLock(lockName));
    }

    private static Lockally _test6Lockally = new();
    [Fact]
    public async Task A_second_lock_request_must_wait_and_acquires_the_lock_if_released()
    {
        var lockName = "LockallyTest7";
        var x = new ConcurrentQueue<string>();
        var thread1 = new Thread(async () =>
        {
            _test6Lockally.Lock(lockName);
            await Task.Delay(100);
            x.Enqueue("1");
            _test6Lockally.Release(lockName);
            await Task.Delay(10);
            x.Enqueue("3");
        });
        var thread2 = new Thread(async () =>
        {
            await Task.Delay(50);
            _test6Lockally.Lock(lockName);
            x.Enqueue("2");
            _test6Lockally.Release(lockName);
        });
        thread1.Start();
        thread2.Start();
        await Task.Delay(150);
        Assert.Equal(new[] { "1", "2", "3" }, x);
        Assert.True(_test6Lockally.TryLock(lockName));
    }

    private static Lockally _test7Lockally = new();
    [Fact]
    public async Task LockAsync_locks()
    {
        await _test7Lockally.LockAsync("LockyTest7");
        Assert.False(_test7Lockally.TryLock("LockyTest7"));
    }

    private static Lockally _test8LockallyTrue = new();
    private static Lockally _test8LockallyFalse = new();
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Lock_can_be_cancelled(bool shouldCancel)
    {
        var lockally = shouldCancel ? _test8LockallyTrue : _test8LockallyFalse;
        var lockName = "LockyTest6" + shouldCancel;
        lockally.Lock(lockName);
        var cancellationTokenSource = new CancellationTokenSource();

        var task = Task.Run(() => lockally.Lock(lockName, cancellationTokenSource.Token));

        if (shouldCancel)
        {
            cancellationTokenSource.Cancel();
        }
        await Task.Delay(50);
        lockally.Release(lockName);
        try
        {
            await task;
        }
        catch
        {
            if (!shouldCancel)
            {
                Assert.Fail("Unexpected Exception.");
            }
        }
        if (shouldCancel)
        {
            Assert.True(lockally.TryLock(lockName));
        }
        else
        {
            Assert.False(lockally.TryLock(lockName));
        }
        cancellationTokenSource.Cancel();
    }

    private static Lockally _test9LockallyTrue = new();
    private static Lockally _test9LockallyFalse = new();
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LockAsync_can_be_cancelled(bool shouldCancel)
    {
        var lockally = shouldCancel ? _test9LockallyTrue : _test9LockallyFalse;

        var lockName = "LockyTest7" + shouldCancel;
        lockally.Lock(lockName);
        var cancellationTokenSource = new CancellationTokenSource();

        var task = Task.Run(async () => await lockally.LockAsync(lockName, cancellationTokenSource.Token));

        if (shouldCancel)
        {
            cancellationTokenSource.Cancel();
        }
        await Task.Delay(50);
        lockally.Release(lockName);
        try
        {
            await task;
        }
        catch
        {
            if (!shouldCancel)
            {
                Assert.Fail("Unexpected Exception.");
            }
        }
        if (shouldCancel)
        {
            Assert.True(lockally.TryLock(lockName));
        }
        else
        {
            Assert.False(lockally.TryLock(lockName));
        }
        cancellationTokenSource.Cancel();
    }
}
using System.Collections.Concurrent;

namespace LockyAndLockally.Tests;

public class LockyTests
{
    [Fact]
    public void Lock_locks()
    {
        // Act
        Locky.Lock("LockyTest1");
        // Assert
        Assert.False(Locky.TryLock("LockyTest1"));

        // Arrange 2
        Locky.Release("LockyTest1");
        // Act 2 - Lock also locks after Release
        Locky.Lock("LockyTest1");
        // Assert 2
        Assert.False(Locky.TryLock("LockyTest1"));
    }

    [Fact]
    public void TryLock_locks()
    {
        // Act & assert 1
        Assert.True(Locky.TryLock("LockyTest2"));
        Assert.False(Locky.TryLock("LockyTest2"));

        // Arrange 2
        Locky.Release("LockyTest2");
        // Act & assert 2 - TryLock also locks after Release
        Assert.True(Locky.TryLock("LockyTest2"));
        Assert.False(Locky.TryLock("LockyTest2"));
    }

    [Fact]
    public async Task Other_threads_work_the_same()
    {
        var tryLockResult = new ConcurrentQueue<bool>();
        Locky.Lock("LockyTest3");
        var thread = new Thread(() => tryLockResult.Enqueue(Locky.TryLock("LockyTest3")));
        thread.Start();
        await Task.Delay(50);
        Assert.False(Assert.Single(tryLockResult));
        Assert.False(Locky.TryLock("LockyTest3"));
    }

    [Fact]
    public async Task A_second_lock_request_must_wait_and_acquires_the_lock_if_released()
    {
        var x = new ConcurrentQueue<string>();
        var thread1 = new Thread(async () =>
        {
            Locky.Lock("LockyTest4");
            await Task.Delay(100);
            x.Enqueue("1");
            Locky.Release("LockyTest4");
            await Task.Delay(10);
            x.Enqueue("3");
        });
        var thread2 = new Thread(async () =>
        {
            await Task.Delay(50);
            Locky.Lock("LockyTest4");
            x.Enqueue("2");
            Locky.Release("LockyTest4");
        });
        thread1.Start();
        thread2.Start();
        await Task.Delay(150);
        Assert.Equal(new[] { "1", "2", "3"}, x);
        Assert.True(Locky.TryLock("LockyTest4"));
    }

    [Fact]
    public async Task LockAsync_locks()
    {
        // Act 1
        await Locky.LockAsync("LockyTest5");
        // Assert 1
        Assert.False(Locky.TryLock("LockyTest5"));

        // Arrange 2
        Locky.Release("LockyTest5");
        // Act 2
        await Locky.LockAsync("LockyTest5");
        // Assert 2
        Assert.False(Locky.TryLock("LockyTest5"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Lock_can_be_cancelled(bool shouldCancel)
    {
        // Arrange
        var lockName = "LockyTest6" + shouldCancel;
        Locky.Lock(lockName);
        var cancellationTokenSource = new CancellationTokenSource();

        var task = Task.Run(() => Locky.Lock(lockName, cancellationTokenSource.Token));

        // Act (or not)
        if (shouldCancel)
        {
            cancellationTokenSource.Cancel();
        }
        await Task.Delay(50);
        Locky.Release(lockName);
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

        // Assert - if waiting for `Lock` has been cancelled, then it has not been locked yet after Release.
        if (shouldCancel)
        {
            Assert.True(Locky.TryLock(lockName));
        }
        else
        {
            Assert.False(Locky.TryLock(lockName));
        }
        cancellationTokenSource.Cancel();
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LockAsync_can_be_cancelled(bool shouldCancel)
    {
        // Arrange
        var lockName = "LockyTest7" + shouldCancel;
        Locky.Lock(lockName);
        var cancellationTokenSource = new CancellationTokenSource();

        var task = Task.Run(async () => await Locky.LockAsync(lockName, cancellationTokenSource.Token));

        // Act (or not)
        if (shouldCancel)
        {
            cancellationTokenSource.Cancel();
        }
        await Task.Delay(50);
        Locky.Release(lockName);
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

        // Assert - if waiting for LockAsync has been cancelled, then it has not been locked yet after Release.
        if (shouldCancel)
        {
            Assert.True(Locky.TryLock(lockName));
        }
        else
        {
            Assert.False(Locky.TryLock(lockName));
        }
        cancellationTokenSource.Cancel();
    }
}
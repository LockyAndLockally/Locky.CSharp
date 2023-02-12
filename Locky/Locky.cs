using System.Collections.Concurrent;

namespace LockyAndLockally;

/// <summary>
/// Have a look at the documentation on https://github.com/LockyAndLockally/Locky.CSharp.
/// </summary>
public static class Locky
{
    private static ConcurrentDictionary<string, SemaphoreSlim> _dict = new();
    private static SemaphoreSlim Slim(string s) => _dict.GetOrAdd(s, new SemaphoreSlim(1, 1));

    /// <summary>
    /// If a lock cannot be acquired immediately, this method returns false and does not block any thread. Usage:
    /// <code>
    /// if (!Locky.TryLock("Process A"))
    /// {
    ///     // important work is already going on, so let's skip it this time.
    ///     return;
    /// }
    /// try
    /// {
    ///     // do some important work...
    /// }
    /// finally
    /// {
    ///     Locky.Release("Process A");
    /// }
    /// </code>    /// </summary>
    public static bool TryLock(string s) => Slim(s).Wait(0);

    /// <summary>
    /// Acquires a lock or waits until it acquires it. The wait is blocking the thread. If you have async code already, then use <see cref="LockAsync"/>. Usage:
    /// <code>
    /// Locky.Lock("Process B");
    /// try
    /// {
    ///     // do some important work...
    /// }
    /// finally
    /// {
    ///     Locky.Release("Process B");
    /// }
    /// </code>
    /// </summary>
    public static void Lock(string s, CancellationToken cancellationToken = default) => Slim(s).Wait(cancellationToken);

    /// <summary>
    /// Acquires a lock or waits until it acquires it. The wait is not blocking, the thread is released. Usage:
    /// <code>
    /// await Locky.LockAsync("Process C");
    /// try
    /// {
    ///     // do some important work...
    /// }
    /// finally
    /// {
    ///     Locky.Release("Process C");
    /// }
    /// </code>
    /// </summary>
    public static Task LockAsync(string s, CancellationToken cancellationToken = default) => Slim(s).WaitAsync(cancellationToken);

    /// <summary>
    /// Always call this method in a <c>finally</c> block and only once (never twice, for example NOT also in a <c>catch</c> block).<br/>
    /// In Visual Studio, place your cursor on this method and use <c>Shift F12</c> to find out what locks are used in this application.
    /// </summary>
    public static void Release(string s) => _dict[s].Release();
}
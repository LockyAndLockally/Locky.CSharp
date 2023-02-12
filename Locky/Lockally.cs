using System.Collections.Concurrent;

namespace LockyAndLockally;

/// <summary>
/// Have a look at the documentation on https://github.com/LockyAndLockally/Locky.CSharp.
/// </summary>
public class Lockally
{
    private ConcurrentDictionary<string, SemaphoreSlim> _dict = new();
    private SemaphoreSlim Slim(string s) => _dict.GetOrAdd(s, new SemaphoreSlim(1, 1));
    /// <summary>
    /// Have a look at the documentation on https://github.com/LockyAndLockally/Locky.CSharp.
    /// </summary>
    public bool TryLock(string s) => Slim(s).Wait(0);
    /// <summary>
    /// Have a look at the documentation on https://github.com/LockyAndLockally/Locky.CSharp.
    /// </summary>
    public void Lock(string s, CancellationToken cancellationToken = default) => Slim(s).Wait(cancellationToken);
    /// <summary>
    /// Have a look at the documentation on https://github.com/LockyAndLockally/Locky.CSharp.
    /// </summary>
    public Task LockAsync(string s, CancellationToken cancellationToken = default) => Slim(s).WaitAsync(cancellationToken);
    /// <summary>
    /// Have a look at the documentation on https://github.com/LockyAndLockally/Locky.CSharp.
    /// </summary>
    public void Release(string s) => _dict[s].Release();
}
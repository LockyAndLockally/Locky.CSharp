using System.Collections.Concurrent;

namespace LockyAndLockally;

/// <summary>
/// Assign to a static field or property.
/// </summary>
public class Lockally
{
    private ConcurrentDictionary<string, SemaphoreSlim> _dict = new();
    private SemaphoreSlim Slim(string s) => _dict.GetOrAdd(s, new SemaphoreSlim(1, 1));
    public bool TryLock(string s) => Slim(s).Wait(0);
    public void Lock(string s, CancellationToken cancellationToken = default) => Slim(s).Wait(cancellationToken);
    public Task LockAsync(string s, CancellationToken cancellationToken = default) => Slim(s).WaitAsync(cancellationToken);
    public void Release(string s) => _dict[s].Release();
}
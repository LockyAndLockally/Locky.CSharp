using System.Collections.Concurrent;

namespace LockyAndLockally;

public static class Locky
{
    private static ConcurrentDictionary<string, SemaphoreSlim> _dict = new();
    private static SemaphoreSlim Slim(string s) => _dict.GetOrAdd(s, new SemaphoreSlim(1, 1));
    public static bool TryLock(string s) => Slim(s).Wait(0);
    public static void Lock(string s, CancellationToken cancellationToken = default) => Slim(s).Wait(cancellationToken);
    public static Task LockAsync(string s, CancellationToken cancellationToken = default) => Slim(s).WaitAsync(cancellationToken);
    public static void Release(string s) => _dict[s].Release();
}
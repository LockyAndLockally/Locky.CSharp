# Locky

All questions in your team about how to properly use locks can be answered with "Use Locky" from now on. It is very easy to use, because you can lock on the `string` type. There is no risk of forgetting to assign something to a static field, because Locky is static itself. Best of all: you can even use it in serious applications!

Note: if you want to use `Locky` in a package, then please use `Lockally` (also included in this package) to avoid clashing with the consumer of your package.

## What is Locky's interface?

```csharp
public static class Locky
{
    public static bool TryLock(string s);
    public static void Lock(string s); // optional: cancellationToken
    public static Task LockAsync(string s); // optional: cancellationToken
    public static void Release(string s);
}
```

## How to use Locky?

In [Visual Studio](https://visualstudio.microsoft.com/downloads/)'s `Solution Explorer`, right-click a project and click `Manage NuGet Packages...`. Browse and install "Locky".

Add
```csharp
using LockyAndLockally;
```

#### Using the `Lock` method

```csharp
Locky.Lock("ProcessA");
try
{
    // do some important work...
}
finally
{
    Locky.Release("ProcessA");
}
```

#### Using the `TryLock` method
```csharp

if (!Locky.TryLock("ProcessB"))
{
    // import work is already going on, so let's skip it this time.
    return;
}
try
{
    // do some important work...
}
finally
{
    Locky.Release("ProcessB");
}
```

#### Using the `LockAsync` method
```csharp
await Locky.LockAsync("ProcessC");
try
{
    // do some important work...
}
finally
{
    Locky.Release("ProcessC");
}
```

## Ok, what's Lockally?

Ehwm... its interface won't surprise either, it's quite simple:

```csharp
public class Lockally
{
    public bool TryLock(string s);
    public void Lock(string s);
    public Task LockAsync(string s);
    public void Release(string s);
}
```

If you're creating a library that unkown users will depend on, you probably don't want to tell them "hey, I've used `"MySuperLock"` as a lock, so you can't use it". If you create a library for your team or for others, then use `Lockally` like this:

```csharp

public class SomeClass
{
    private static Lockally _lockally = new();

    public void SomeMethod()
    {
        if (!lockally.TryLock("ProcessA"))
        {
            // import work is already going on, so let's skip it this time.
            return; 
        }
        try
        {
            // do some important work...
        }
        finally
        {
            Locky.Release("ProcessA");
        }
    }
}
```
or make a singleton available within only your library:
```csharp
internal static class MyLocky
{
    public static Lockally InMyLibrary { get; } = new();
}
```
and use it like this:
```csharp
MyLocky.InMyLibrary.Lock("Hello world!");
```

## Tip

In Visual Studio, place your cursor on the `Release` method and use `Shift F12` to find out what lock names have been used by your colleagues.
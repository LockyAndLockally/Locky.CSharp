# Locky

All questions in your team about how to properly use locks can be answered with "use Locky" from now on. It is very easy to use, because you can lock on strings via synchronous and asynchronous methods. There is no risk of forgetting to assign something to a static field, because `Locky` is static itself (or use `Lockally` which is also included).

Notes:

- if you want to use Locky in a package, then please use `Lockally` (also included in this package) to avoid clashing with the consumer of your package.<br/>
- once a string has been used as a lock, it is not removed from the dictionary that is used internally (until your app restarts). Using Locky for a few thousand different strings is probably OK, but start to think about memory usage if you have variable locks such as contract Ids.

<p align="center">
    <img src="https://avatars.githubusercontent.com/u/125100496?s=100&u=dfb896642d9b9e298628e8dd804202ed3c5e1386&v=4" alt="Locky logo"/>
</p>

Available via [NuGet](https://www.nuget.org/packages/Locky).

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
Locky.Lock("Process A");
try
{
    // do some important work...
}
finally
{
    Locky.Release("Process A");
}
```

#### Using the `TryLock` method
```csharp

if (!Locky.TryLock("Process B"))
{
    // important work is already going on, so let's skip it this time.
    return;
}
try
{
    // do some important work...
}
finally
{
    Locky.Release("Process B");
}
```

#### Using the `LockAsync` method
```csharp
await Locky.LockAsync("Process C");
try
{
    // do some important work...
}
finally
{
    Locky.Release("Process C");
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
        if (!_lockally.TryLock("Process D"))
        {
            // important work is already going on, so let's skip it this time.
            return; 
        }
        try
        {
            // do some important work...
        }
        finally
        {
            _lockally.Release("Process D");
        }
    }
}
```
or make a singleton available within only your library in two steps:

Step 1: make an internal static container for a `Lockally` instance.
```csharp
using LockyAndLockally;
namespace MyLibrary;

internal static class MyLockyContainer
{
    public static Lockally MyLocky { get; } = new();
}
```
Step 2: add the container as a static global using to your 'Usings.cs':
```csharp
global using static MyLibrary.MyLockyContainer;
```
and use it like this anywhere in your project:
```csharp
MyLocky.Lock("Hello world!");
```

## Tip

In Visual Studio, place your cursor on the `Release` method and use `Shift F12` to find out what lock names have been used by your colleagues.

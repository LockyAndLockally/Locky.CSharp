using LockyAndLockally;

namespace MyLibrary;

public class MyLockyTests
{
    [Fact]
    public void MyLocky_can_be_made_avaiable_via_global_using_static()
    {
        Assert.True(MyLocky.TryLock("See 'global using static MyLibrary.MyLockyContainer' in 'Usings.cs'!"));
    }
}

internal static class MyLockyContainer
{
    public static Lockally MyLocky { get; } = new();
}
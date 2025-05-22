using NUnit.Framework;
using UnityEssentials;

public class ManagedArrayTests
{
    private struct TestStruct
    {
        public int Value;
    }

    [Test]
    public void Constructor_InitializesCorrectly()
    {
        var array = new ManagedArray<TestStruct>(4);
        Assert.AreEqual(0, array.Count);
    }

    [Test]
    public void Get_IncreasesCount()
    {
        var array = new ManagedArray<TestStruct>(4);
        ref var element = ref array.Get(out var index);
        element.Value = 123;
        Assert.AreEqual(1, array.Count);
        Assert.AreEqual(123, array.Elements[0].Value);
    }

    [Test]
    public void Return_DecreasesCount()
    {
        var array = new ManagedArray<TestStruct>(4);
        ref var element = ref array.Get(out var index);
        array.Return(index);
        Assert.AreEqual(0, array.Count);
    }

    [Test]
    public void Return_InvalidIndex_Throws()
    {
        var array = new ManagedArray<TestStruct>(4);
        Assert.Throws<System.ArgumentOutOfRangeException>(() => array.Return(-1));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => array.Return(5));
    }

    [Test]
    public void Return_WhenEmpty_Throws()
    {
        var array = new ManagedArray<TestStruct>(4);
        Assert.Throws<System.InvalidOperationException>(() => array.Return(0));
    }

    [Test]
    public void Get_WhenFull_Resizes()
    {
        var array = new ManagedArray<TestStruct>(2);

        ref var a = ref array.Get(out _); a.Value = 1;
        ref var b = ref array.Get(out _); b.Value = 2;
        ref var c = ref array.Get(out _); c.Value = 3;

        Assert.AreEqual(3, array.Count);

        int found = 0;
        for (int i = 0; i < array.Elements.Length; i++)
        {
            int value = array.Elements[i].Value;
            if (value == 1 || value == 2 || value == 3)
                found += value;
        }

        Assert.AreEqual(6, found);
    }

    [Test]
    public void Constructor_WithInvalidCapacity_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => new ManagedArray<TestStruct>(0));
    }
}

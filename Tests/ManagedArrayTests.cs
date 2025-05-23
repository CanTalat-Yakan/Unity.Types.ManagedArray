using NUnit.Framework;
using System;

namespace UnityEssentials.Tests
{
    [TestFixture]
    public class ManagedArrayTests
    {
        [Test]
        public void Constructor_DefaultCapacity_InitializesCorrectly()
        {
            var array = new ManagedArray<int>();
            Assert.NotNull(array.Elements);
            Assert.AreEqual(256, array.Elements.Length);
            Assert.AreEqual(0, array.Count);
        }

        [Test]
        public void Constructor_CustomCapacity_InitializesCorrectly()
        {
            var array = new ManagedArray<int>(10);
            Assert.NotNull(array.Elements);
            Assert.AreEqual(10, array.Elements.Length);
            Assert.AreEqual(0, array.Count);
        }

        [Test]
        public void Constructor_ThrowsOnInvalidCapacity()
        {
            Assert.Throws<ArgumentException>(() => new ManagedArray<int>(0));
            Assert.Throws<ArgumentException>(() => new ManagedArray<int>(-5));
        }

        [Test]
        public void Get_AllocatesElementAndReturnsIndex()
        {
            var array = new ManagedArray<int>(4);
            ref int value = ref array.Get(out var index);
            Assert.AreEqual(0, index);
            Assert.AreEqual(1, array.Count);
            value = 42;
            Assert.AreEqual(42, array.Elements[index]);
        }

        [Test]
        public void Get_MultipleAllocations_UsesFreeList()
        {
            var array = new ManagedArray<int>(2);
            array.Get(out var index1);
            array.Get(out var index2);
            Assert.AreNotEqual(index1, index2);
            Assert.AreEqual(2, array.Count);
        }

        [Test]
        public void Get_TriggersResizeWhenFull()
        {
            var array = new ManagedArray<int>(1);
            array.Get(out var index1);
            // Next allocation should trigger resize
            array.Get(out var index2);
            Assert.AreEqual(2, array.Elements.Length);
            Assert.AreEqual(2, array.Count);
        }

        [Test]
        public void Return_ReleasesElementAndDecrementsCount()
        {
            var array = new ManagedArray<int>(3);
            array.Get(out var index);
            Assert.AreEqual(1, array.Count);
            array.Return(index);
            Assert.AreEqual(0, array.Count);
            // Should be able to reuse the same index
            array.Get(out var index2);
            Assert.AreEqual(index, index2);
        }

        [Test]
        public void Return_ThrowsOnInvalidIndex()
        {
            var array = new ManagedArray<int>(2);
            array.Get(out var index);
            Assert.Throws<ArgumentOutOfRangeException>(() => array.Return(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.Return(2));
        }

        [Test]
        public void Return_ThrowsWhenNoElementsInUse()
        {
            var array = new ManagedArray<int>(2);
            Assert.Throws<InvalidOperationException>(() => array.Return(0));
        }

        [Test]
        public void GetAndReturn_MultipleCycles_WorkAsExpected()
        {
            var array = new ManagedArray<int>(2);
            array.Get(out var index1);
            array.Get(out var index2);
            array.Return(index1);
            array.Return(index2);
            Assert.AreEqual(0, array.Count);
            array.Get(out var index3);
            array.Get(out var index4);
            Assert.AreNotEqual(index3, index4);
            Assert.Contains(index3, new[] { 0, 1 });
            Assert.Contains(index4, new[] { 0, 1 });
        }
    }
}
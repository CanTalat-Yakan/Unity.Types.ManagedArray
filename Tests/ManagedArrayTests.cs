using NUnit.Framework;
using System;
using static UnityEditor.Experimental.GraphView.Port;

namespace UnityEssentials.Tests
{
    [TestFixture]
    public class ManagedArrayTests
    {
        [Test]
        public void Constructor_DefaultCapacity_InitializesCorrectly()
        {
            var arr = new ManagedArray<int>();
            Assert.NotNull(arr.Elements);
            Assert.AreEqual(256, arr.Elements.Length);
            Assert.AreEqual(0, arr.Count);
        }

        [Test]
        public void Constructor_CustomCapacity_InitializesCorrectly()
        {
            UnityEngine.Debug.Log($"test");
            var arr = new ManagedArray<int>(10);
            UnityEngine.Debug.Log($"test2");
            Assert.NotNull(arr.Elements);
            Assert.AreEqual(10, arr.Elements.Length);
            Assert.AreEqual(0, arr.Count);
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
            var arr = new ManagedArray<int>(4);
            int index;
            ref int value = ref arr.Get(out index);
            Assert.AreEqual(0, index);
            Assert.AreEqual(1, arr.Count);
            value = 42;
            Assert.AreEqual(42, arr.Elements[index]);
        }

        [Test]
        public void Get_MultipleAllocations_UsesFreeList()
        {
            var arr = new ManagedArray<int>(2);
            int idx1, idx2;
            arr.Get(out idx1);
            arr.Get(out idx2);
            Assert.AreNotEqual(idx1, idx2);
            Assert.AreEqual(2, arr.Count);
        }

        [Test]
        public void Get_TriggersResizeWhenFull()
        {
            var arr = new ManagedArray<int>(1);
            int idx1, idx2;
            arr.Get(out idx1);
            // Next allocation should trigger resize
            arr.Get(out idx2);
            Assert.AreEqual(2, arr.Elements.Length);
            Assert.AreEqual(2, arr.Count);
        }

        [Test]
        public void Return_ReleasesElementAndDecrementsCount()
        {
            var arr = new ManagedArray<int>(3);
            int idx;
            arr.Get(out idx);
            Assert.AreEqual(1, arr.Count);
            arr.Return(idx);
            Assert.AreEqual(0, arr.Count);
            // Should be able to reuse the same index
            int idx2;
            arr.Get(out idx2);
            Assert.AreEqual(idx, idx2);
        }

        [Test]
        public void Return_ThrowsOnInvalidIndex()
        {
            var arr = new ManagedArray<int>(2);
            int idx;
            arr.Get(out idx);
            Assert.Throws<ArgumentOutOfRangeException>(() => arr.Return(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => arr.Return(2));
        }

        [Test]
        public void Return_ThrowsWhenNoElementsInUse()
        {
            var arr = new ManagedArray<int>(2);
            Assert.Throws<InvalidOperationException>(() => arr.Return(0));
        }

        [Test]
        public void GetAndReturn_MultipleCycles_WorkAsExpected()
        {
            var arr = new ManagedArray<int>(2);
            int idx1, idx2;
            arr.Get(out idx1);
            arr.Get(out idx2);
            arr.Return(idx1);
            arr.Return(idx2);
            Assert.AreEqual(0, arr.Count);
            int idx3, idx4;
            arr.Get(out idx3);
            arr.Get(out idx4);
            Assert.AreNotEqual(idx3, idx4);
            Assert.Contains(idx3, new[] { 0, 1 });
            Assert.Contains(idx4, new[] { 0, 1 });
        }
    }
}
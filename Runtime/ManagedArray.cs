using System;
using UnityEngine;

namespace UnityEssentials
{
    /// <summary>
    /// Represents a managed collection of elements with efficient allocation and deallocation capabilities.
    /// </summary>
    /// <remarks>The <see cref="ManagedArray{T}"/> struct provides a fixed-capacity array with efficient reuse
    /// of unused slots. It is designed for scenarios where frequent allocation and deallocation of elements are
    /// required, such as object pooling or memory management systems. The collection automatically resizes when
    /// additional capacity is needed.</remarks>
    /// <typeparam name="T">The type of elements stored in the collection. Must be a value type (<see langword="struct"/>).</typeparam>
    public class ManagedArray<T> where T : struct
    {
        public T[] Elements;
        public int Count { get; private set; }

        private int[] _nextFree;
        private int _freeHead;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedArray{T}"/> class with the specified capacity.
        /// </summary>
        /// <remarks>The <see cref="ManagedArray{T}"/> class manages an array of elements with a fixed
        /// capacity,  providing efficient allocation and deallocation of elements. The internal structure ensures  that
        /// unused slots are tracked for reuse.</remarks>
        /// <param name="capacity">The initial capacity of the array. Must be at least 1. Defaults to 256 if not specified.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is less than 1.</exception>
        public ManagedArray(int capacity = 256)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

            Elements = new T[capacity];
            Count = 0;

            _nextFree = new int[capacity];
            for (int i = 0; i < capacity - 1; i++)
                _nextFree[i] = i + 1;
            _nextFree[capacity - 1] = -1;
            _freeHead = 0;
        }

        /// <summary>
        /// Retrieves a reference to an element from the collection and provides its index.
        /// </summary>
        /// <remarks>If the collection has no available elements, it automatically resizes to accommodate
        /// the request.</remarks>
        /// <param name="index">When this method returns, contains the index of the retrieved element in the collection.</param>
        /// <returns>A reference to the retrieved element in the collection.</returns>
        public ref T Get(out int index)
        {
            if (_freeHead == -1)
                Resize();

            index = _freeHead;
            _freeHead = _nextFree[index];
            Count++;
            return ref Elements[index];
        }

        /// <summary>
        /// Returns an element to the pool, making it available for reuse.
        /// </summary>
        /// <param name="index">The index of the element to return. Must be within the valid range of the pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the length of the pool.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there are no elements currently in use to return.</exception>
        public void Return(int index)
        {
            if (index < 0 || index >= Elements.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (Count <= 0)
                throw new InvalidOperationException("No elements to return.");

            _nextFree[index] = _freeHead;
            _freeHead = index;
            Count--;
        }

        /// <summary>
        /// Increases the capacity of the internal arrays to accommodate additional elements.
        /// </summary>
        /// <remarks>This method resizes the internal arrays to a larger capacity, ensuring that
        /// additional space is  available for new elements. The new capacity is calculated as 1.5 times the current
        /// capacity.  The method also updates the internal free list to reflect the newly available slots.</remarks>
        private void Resize()
        {
            int oldCapacity = Elements.Length;
            int newCapacity = oldCapacity + Mathf.Max(1, oldCapacity >> 1);

            Array.Resize(ref Elements, newCapacity);
            Array.Resize(ref _nextFree, newCapacity);

            for (int i = oldCapacity; i < newCapacity; i++)
                _nextFree[i] = i + 1;

            _nextFree[newCapacity - 1] = -1;
            _freeHead = oldCapacity;
        }
    }
}
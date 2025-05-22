using System;

namespace UnityEssentials
{
    public struct ManagedArray<T> where T : struct
    {
        public T[] Elements;
        public int Count { get; private set; }

        private int[] _nextFree;
        private int _freeHead;

        public ManagedArray(int capacity = 256)
        {
            if (capacity < 1)
                throw new ArgumentException("Capacity must be at least 1.", nameof(capacity));

            Elements = new T[capacity];
            _nextFree = new int[capacity];
            for (int i = 0; i < capacity - 1; i++)
                _nextFree[i] = i + 1;
            _nextFree[capacity - 1] = -1;
            _freeHead = 0;
            Count = 0;
        }

        public ref T Get(out int index)
        {
            if (_freeHead == -1)
                Resize();

            index = _freeHead;
            _freeHead = _nextFree[index];
            Count++;
            return ref Elements[index];
        }

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

        private void Resize()
        {
            int oldCapacity = Elements.Length;
            int newCapacity = oldCapacity + (oldCapacity / 2);

            Array.Resize(ref Elements, newCapacity);
            Array.Resize(ref _nextFree, newCapacity);

            for (int i = oldCapacity; i < newCapacity; i++)
                _nextFree[i] = i + 1;

            _nextFree[newCapacity - 1] = -1;
            _freeHead = oldCapacity;
        }
    }
}
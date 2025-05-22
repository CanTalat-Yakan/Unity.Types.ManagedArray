using System;

public struct ManagedArray<T> where T : struct
{
    public T[] Items;
    public int Count;

    public int[] NextFree;
    public int FreeHead;

    public ManagedArray(int capacity)
    {
        Items = new T[capacity];
        NextFree = new int[capacity];
        Count = 0;
        FreeHead = -1;
    }

    public int Add(T item)
    {
        int index;
        if (FreeHead >= 0)
        {
            index = FreeHead;
            FreeHead = NextFree[index];
        }
        else
        {
            if (Count >= Items.Length)
            {
                Array.Resize(ref Items, Items.Length * 2);
                Array.Resize(ref NextFree, Items.Length);
            }
            index = Count++;
        }
        Items[index] = item;
        return index;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count) return;
        NextFree[index] = FreeHead;
        FreeHead = index;
    }
}

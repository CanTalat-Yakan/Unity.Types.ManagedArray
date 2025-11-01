# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Managed Array

> Quick overview: A pooled, growable array for structs. Get a ref to a free slot with its index, write into it, and Return the index to free it. Backed by a free‑list for O(1) allocate/free and automatic 1.5× growth.

`ManagedArray<T>` is a lightweight container for value types (structs) that minimizes allocations and enables fast reuse of slots. It’s ideal for high‑frequency systems like schedulers or job queues where you need to get a reference to a slot, write to it, and later return it for reuse.

![screenshot](Documentation/Screenshot.png)

## Features
- Pooled, indexed storage for structs
  - `Get(out int index)` returns a ref to a free element and its index
  - `Return(index)` frees the slot for reuse
- Efficient growth
  - Auto‑resizes the internal arrays by ~1.5× when exhausted
- O(1) allocate/free via free‑list
  - Internal `_nextFree` chain and `_freeHead` pointer
- Minimal overhead
  - No per‑element heap allocations; struct storage in a backing array
- Simple accounting
  - `Count` tracks how many slots are currently in use

## Requirements
- Unity 6000.0+
- Runtime module; no external dependencies
- Generic type parameter must be a struct: `ManagedArray<T> where T : struct`

## Usage

### Define your element and create the array
```csharp
public struct Process
{
    public int Id;
    public float StartTime;
}

var pool = new ManagedArray<Process>(capacity: 256); // default is 256
```

### Allocate, write, and free
```csharp
// Allocate a slot and get its index
ref var proc = ref pool.Get(out int idx);
proc.Id = 42;
proc.StartTime = Time.time;

// ... later, when done with this slot
pool.Return(idx);
```

### Auto‑resize behavior
```csharp
// When no free slots remain, Get() grows the backing arrays by ~1.5× automatically
for (int i = 0; i < 10_000; i++)
{
    ref var e = ref pool.Get(out int index);
    e.Id = i;
}
```

## API at a glance
- `public class ManagedArray<T> where T : struct`
  - Fields
    - `T[] Elements` – backing storage (public for direct access if needed)
  - Properties
    - `int Count` – number of slots currently in use
  - Constructors
    - `ManagedArray(int capacity = 256)` – throws if capacity < 1
  - Methods
    - `ref T Get(out int index)` – returns a ref to a free slot, increments `Count`; resizes if necessary
    - `void Return(int index)` – frees the slot, decrements `Count`; throws if index out of range or `Count` <= 0

## How It Works
- Free‑list management
  - `_nextFree[i]` stores the next free index; `_freeHead` points to the first free slot
  - `Get` pops from `_freeHead` and returns a ref to `Elements[index]`
  - `Return` pushes `index` back onto the free list
- Growth strategy
  - When `_freeHead == -1`, `Resize()` expands arrays to `old + max(1, old >> 1)` and links the new indices into the free list

## Notes and Limitations
- Value types only: `T` must be a struct
- Not thread‑safe: use on the main thread or protect with your own synchronization
- No shrink: the backing arrays don’t contract; reuse freed slots instead
- No “is used” query per index: only `Count` is exposed; track your indices to know what’s active
- Elements are not cleared on `Return`: write over old data when reusing a slot
- Bounds and state checks
  - `Return` throws `ArgumentOutOfRangeException` for invalid indices and `InvalidOperationException` if no elements are in use

## Files in This Package
- `Runtime/ManagedArray.cs` – Implementation of the pooled array and free‑list
- `Runtime/UnityEssentials.ManagedArray.asmdef` – Runtime assembly definition
- `Tests/UnityEssentials.ManagedArray.Tests.asmdef` – Test assembly definition (tests optional)

## Tags
unity, array, pool, freelist, memory, performance, structs, runtime, systems

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBuffer<T> {
    private T[] Data;
    private int startingIndex = 0;
    public int Count { get; protected set; } = 0;
    public int Size { get; protected set; }
    public RingBuffer(int Size) {
        if (Size < 0) {
            throw new System.ArgumentException($"Cannot create a buffer with size {Size}");
        }
        this.Size = Size;
        Data = new T[Size];
    }

    public void Add(T value) {
        if (Count < Size) {
            Data[startingIndex + Count++] = value;
        }
        else {
            Data[startingIndex] = value;
            startingIndex = (startingIndex + 1) % Size;
        }
    }

    public void Clear() {
        // Just leave the data alone I guess?
        Count = 0;
        startingIndex = 0;
    }

    public T this[int index] {
        get {
            if (index > Count) {
                throw new System.IndexOutOfRangeException($"Cannot access the {index} element of a ring buffer with only {Count} elements");
            }
            return Data[(startingIndex + index) % Size];
        }
    }
}
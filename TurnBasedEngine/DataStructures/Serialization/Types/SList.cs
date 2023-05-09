using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[TypeIndex(0)]
public class UnsafeList<RootType> : ISerializable, IEnumerable<RootType>, IList, IGenerateable
{
    [HideInInspector]
    public List<RootType> innerList;

    public UnsafeList() {
        innerList = new List<RootType>();
    }
    public UnsafeList(object list) : this() {
        if (list is IEnumerable) {
            foreach (object element in (list as IEnumerable)) {
                if (element is RootType) {
                    innerList.Add((RootType)element);
                }
            }
        }
    }
    public UnsafeList(params RootType[] elements) : this() {
        foreach (var element in elements) {
            innerList.Add(element);
        }
    }

    public void DeSerialize(Stream stream) {
        var length = stream.ConsumeInt();
        innerList = new List<RootType>(length);
        for (int i = 0; i < length; i++) {
            innerList.Add(default(RootType));
        }
        for (int i = 0; i < length; i++) {
            var value = stream.Consume();
            if (value is RootType) {
                innerList[i] = (RootType)value;
            }
            else if (SNull.IsNull(value)) {
                // This will end up being SNull for reasons, and that's probably incompatible with RootType, so set it back ot null
                continue;
            }
            else {
                throw new Exception($"Serialization claimed that {value.GetType()} is of type {typeof(RootType)}.");
            }
        }
    }

    public int SerializationSize() {
        int lengthSize = Stream.IntSize();
        int totalSize = lengthSize;
        foreach (RootType element in innerList) {
            totalSize += Stream.SerializationSize(element);
        }
        return totalSize;
    }

    public void Serialize(Stream stream) {
        stream.WriteInt(innerList.Count);
        foreach (RootType element in innerList) {
            stream.Write(element);
        }
    }
    public RootType GetRaw(int key) {
        return key >= innerList.Count ? default(RootType) : innerList[key];
    }
    public void SetRaw(int key, RootType val) {
        if (SNull.IsNull(val)) {
            Remove(key);
            return;
        }
        Assert.That(val is ISerializable, $"Not serializable object encountered!");
        while (key >= innerList.Count) {
            innerList.Add(default(RootType));
        }
        innerList[key] = val;
    }
    public void Set<T>(int key, T newValue) where T : ISerializable, RootType {
        RootType existing = key < innerList.Count ? innerList[key] : default(RootType);
        if (existing?.Equals(newValue) == true) {
            return;
        }
        if (!SNull.IsNull(existing)) {
            ModelViewController.Instance.PreSerializableUnlinked(this, (ISerializable)existing, key);
        }
        SetRaw(key, newValue);
        ModelViewController.Instance.OnSerializableLinked(this, (ISerializable)newValue, key);
    }
    public T Get<T>(int key) where T : RootType {
        return Get<T>(key, default(T));
    }
    protected T Get<T>(int key, T defaultValue) where T : RootType {
        while (key >= innerList.Count) {
            innerList.Add(default(RootType));
        }
        var value = innerList[key];
        if (!SNull.IsNull(value)) {
            if (value is T) {
                return (T)value;
            }
            throw new InvalidOperationException(string.Format("Key: {0} has value of unexpected type: {1}.\n Type: {3}\n Value: {2}", key, value.GetType(), value, typeof(T)));
        }
        else {
            return default(T);
        }
    }

    public RootType Pop() {
        var first = innerList[0];
        innerList.RemoveAt(0);
        return first;
    }
    public void Add<T>(T element) where T : ISerializable, RootType {
        innerList.Add(element);
        ModelViewController.Instance.OnSerializableLinked(this, element);
    }
    public void AddRange<T>(IEnumerable<T> elements) where T : ISerializable, RootType {
        foreach (var element in elements) {
            Add(element);
        }
    }
    public void Remove(int key) {
        if (key >= innerList.Count) {
            return;
        }
        var value = innerList[key];
        if (value == null) {
            return;
        }
        ModelViewController.Instance.PreSerializableUnlinked(this, (ISerializable)value, key);
        innerList[key] = default(RootType);
        ModelViewController.Instance.OnSerializableRemoved(this, key);
    }
    public void Clear() {
        for (int i = innerList.Count - 1; i >= 0; i--) {
            var element = innerList[i];
            if (element is ISerializable) {
                ModelViewController.Instance.PreSerializableUnlinked(this, (ISerializable)element);
                ModelViewController.Instance.OnSerializableRemoved(this, i);
            }
        }
        innerList.Clear();
    }
    public IEnumerable<RootType> GetEnumerable() {
        return innerList;
    }

    public IEnumerator<RootType> GetEnumerator() {
        return innerList.GetEnumerator();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public object this[int index] { get => innerList[index]; set => ((IList)innerList)[index] = value; }

    public bool IsFixedSize { get => false; }
    public bool IsReadOnly { get => false; }

    public int Add(object value) => ((IList)innerList).Add(value);
    public bool Contains(object value) => ((IList)innerList).Contains(value);
    public int IndexOf(object value) => ((IList)innerList).IndexOf(value);
    public void Insert(int index, object value) => ((IList)innerList).Insert(index, value);
    public void Remove(object value) => ((IList)innerList).Remove(value);
    public void RemoveAt(int index) => ((IList)innerList).RemoveAt(index);
    public bool IsSynchronized { get => ((ICollection)innerList).IsSynchronized; }
    public object SyncRoot { get => ((ICollection)innerList).SyncRoot; }
    public void CopyTo(Array array, int index) => ((ICollection)innerList).CopyTo(array, index);
    public int Count { get { return innerList.Count; } }
    public int IndexOf<T>(T value) where T : RootType {
        return innerList.IndexOf(value);
    }

    public override string ToString() {
        var stringValues = innerList.Select(x => x == null ? "<null>" : x.ToString());
        var listCenter = string.Join(", ", stringValues);
        return $"[{listCenter}]";
    }
}

[TypeIndex(1)]
public class SList<T> : UnsafeList<T>, IIndexable<T>, IPoolable
    where T : ISerializable
{
    public new T this[int key] {
        get {
            return GetRaw(key);
        }
        set {
            while (key > innerList.Count() - 1) {
                innerList.Add(default(T));
            }
            innerList[key] = value;
        }
    }
    public SList() : base() {
    }
    public SList(object list) : base(list) {
    }
    public SList(params T[] elements) : base(elements) {
    }
    public void Return() {
        foreach (var val in innerList) {
            (val as IPoolable)?.Return();
        }
        innerList.Clear();
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
        innerList.Clear();
    }
    public void Trim() {
        var lastFullIndex = innerList.Count - 1;
        for (; lastFullIndex >= 0 && SNull.IsNull(innerList[lastFullIndex]); lastFullIndex--) ;
        innerList.RemoveRange(lastFullIndex + 1, innerList.Count - lastFullIndex - 1);
    }
}
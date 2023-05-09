using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(238)]
public class SShort : ISerializable, ICastable<ushort>, IGenerateable, IPoolable {
    public ushort Value;
    public SShort() : this(0) { }
    public SShort(ushort Value) {
        this.Value = Value;
    }
    public void DeSerialize(Stream stream) {
        Value = stream.ConsumeShort();
    }

    public int SerializationSize() {
        return Stream.ShortSize();
    }

    public void Serialize(Stream stream) {
        stream.WriteLong(Value);
    }

    ushort ICastable<ushort>.Cast() {
        return Value;
    }
    public void Return() {
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
    }
    public static implicit operator ushort(SShort b) {
        Assert.That(b != null, "Attempted to read from an int value that's never been written!");
        return b.Value;
    }
    public static implicit operator SShort(int b) {
        return new SShort((ushort)b);
    }
    public static implicit operator SShort(ushort b) {
        return new SShort(b);
    }
    public static implicit operator String(SShort b) {
        Assert.That(b != null, $"Attempted to read from an int value that's never been written!");
        return b.Value.ToString();
    }
    public override bool Equals(object obj) {
        if (obj as SShort is var o && o != null) {
            return Value == o.Value;
        }
        return Value.Equals(obj);
    }
    public override int GetHashCode() {
        return Value.GetHashCode();
    }
    public override string ToString() {
        return Value.ToString();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(42)]
public class SFloat : ISerializable, ICastable<float>, IGenerateable, IPoolable {
    public Single Value;
    public SFloat() : this(0) { }
    public SFloat(Single Value) {
        this.Value = Value;
    }
    public void DeSerialize(Stream stream) {
        Value = stream.ConsumeFloat();
    }

    public int SerializationSize() {
        return Stream.FloatSize();
    }

    public void Serialize(Stream stream) {
        stream.WriteFloat(Value);
    }

    float ICastable<float>.Cast() {
        return Value;
    }
    public void Return() {
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
        Value = 0;
    }
    public static implicit operator float(SFloat b) {
        Assert.That(b != null, "Attempted to read from an float value that's never been written!");
        return b.Value;
    }
    public static implicit operator float?(SFloat b) {
        Single? r = null;
        if (b != null) {
            r = b.Value;
        }
        return r;
    }
    public static implicit operator SFloat(float b) {
        return new SFloat(b);
    }
    public static implicit operator String(SFloat b) {
        Assert.That(b != null, $"Attempted to read from an float value that's never been written!");
        return b.Value.ToString();
    }
    public override bool Equals(object obj) {
        if (obj as SFloat is var o && o != null) {
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
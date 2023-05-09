using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(10)]
public class SColor : ISerializable, ICastable<Color>, IGenerateable, IPoolable {
    public float[] Value;
    public SColor() {
        Value = new float[4];
    }
    public SColor(Color Value) {
        this.Value = new float[] { Value.r, Value.g, Value.b, Value.a };
    }
    public void DeSerialize(Stream stream) {
        Buffer.BlockCopy(stream.data, stream.index, Value, 0, 16);
        stream.index += 16;
    }

    public int SerializationSize() {
        return 16;
    }

    public void Serialize(Stream stream) {
        Buffer.BlockCopy(Value, 0, stream.data, stream.index, 16);
        stream.index += 16;
    }

    Color ICastable<Color>.Cast() {
        return new Color(Value[0], Value[1], Value[2], Value[3]);
    }
    public void Return() {
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
    }
    public static implicit operator SColor(Color b) {
        return new SColor(b);
    }
    public static implicit operator Color(SColor b) {
        Assert.That(b != null, $"Attempted to read from a color value that's never been written!");
        return ((ICastable<Color>)b).Cast();
    }
    public override bool Equals(object obj) {
        if (obj as SColor is var o && o != null) {
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
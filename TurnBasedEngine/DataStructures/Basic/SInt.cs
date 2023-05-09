using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(6)]
public class SInt : ISerializable, ICastable<int>, IGenerateable, IPoolable {
    [GenerateInt]
    public Int32 Value;
    public SInt() : this(0) {}
    public SInt(Int32 Value) {
        this.Value = Value;
    }
    public void DeSerialize(Stream stream) {
        Value = stream.ConsumeInt();
    }

    public int SerializationSize() {
        return Stream.IntSize();
    }

    public void Serialize(Stream stream) {
        stream.WriteInt(Value);
    }

    int ICastable<int>.Cast() {
        return Value;
    }
    public void Return() {
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
        Value = 0;
    }
    public static implicit operator int(SInt b) {
        Assert.That(b != null, "Attempted to read from an int value that's never been written!");
        return b.Value;
    }
    public static implicit operator int?(SInt b) {
        int? r = null;
        if (b != null) {
            r = b.Value;
        }
        return r;
    }
    public static implicit operator SInt(int b) {
        return new SInt(b);
    }
    public static implicit operator String(SInt b) {
        Assert.That(b != null, $"Attempted to read from an int value that's never been written!");
        return b.Value.ToString();
    }
    public override bool Equals(object obj) {
        if (obj as SInt is var o && o != null) {
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
    public static SInt Today() {
        return (DateTime.Today - new DateTime(1970, 1, 1)).Days;
    }
}

public class GenerateInt : Generate {
    protected int max;
    protected int min;
    public GenerateInt() : this(0, 4) {
    }
    public GenerateInt(int min, int max) {
        this.min = min;
        this.max = max;
    }
    public override void FillIn(object obj, System.Reflection.MemberInfo memberInfo) {
        Int32 value = RNG.Generation.Roll(max - min + 1) + min - 1;
        SetMember(obj, memberInfo, value);
    }
}
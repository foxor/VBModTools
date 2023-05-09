using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(9)]
public class SLong : ISerializable, ICastable<ulong>, IGenerateable, IPoolable {
    [GenerateLong]
    public ulong Value;
    public SLong() : this(0) {}
    public SLong(ulong Value) {
        this.Value = Value;
    }
    public void DeSerialize(Stream stream) {
        Value = stream.ConsumeLong();
    }

    public int SerializationSize() {
        return Stream.LongSize();
    }

    public void Serialize(Stream stream) {
        stream.WriteLong(Value);
    }

    ulong ICastable<ulong>.Cast() {
        return Value;
    }
    public void Return() {
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
    }
    public static implicit operator ulong(SLong b) {
        Assert.That(b != null, "Attempted to read from an int value that's never been written!");
        return b.Value;
    }
    public static implicit operator SLong(long b) {
        return new SLong((ulong)b);
    }
    public static implicit operator SLong(ulong b) {
        return new SLong(b);
    }
    public static implicit operator String(SLong b) {
        Assert.That(b != null, $"Attempted to read from an int value that's never been written!");
        return b.Value.ToString();
    }
    public override bool Equals(object obj) {
        if (obj as SLong is var o && o != null) {
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

public class GenerateLong : Generate {
    protected int max;
    protected int min;
    public GenerateLong() : this(0, 4) {
    }
    public GenerateLong(int min, int max) {
        this.min = min;
        this.max = max;
    }
    public override void FillIn(object obj, System.Reflection.MemberInfo memberInfo) {
        ulong value = (ulong)(RNG.Generation.Roll(max - min + 1) + min - 1);
        SetMember(obj, memberInfo, value);
    }
}
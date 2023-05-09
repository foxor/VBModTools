using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(4)]
public class SBool : ISerializable, ICastable<bool>, IGenerateable {
    [GenerateBool]
    public bool Value;
    public SBool() : this(false) {}
    public SBool(bool Value) {
        this.Value = Value;
    }
    public void DeSerialize(Stream stream) {
        Value = stream.ConsumeByte() != 0;
    }
    public int SerializationSize() {
        return 1;
    }

    public void Serialize(Stream stream) {
        stream.WriteByte(Value ? (byte)1 : (byte)0);
    }
    public bool Cast() {
        return Value;
    }
    public static implicit operator bool(SBool b) {
        Assert.That(b != null, $"Attempted to read a bool value that has never been written");
        return b.Value;
    }
    public static implicit operator SBool(bool b) {
        return new SBool(){Value = b};
    }
    public override bool Equals(object obj) {
        if (obj is SBool other) {
            return Value == other.Value;
        }
        return Cast().Equals(obj);
    }
    public override int GetHashCode() {
        return Cast().GetHashCode();
    }
    public override string ToString() {
        return Cast().ToString();
    }
}

public class GenerateBool : Generate {
    public override void FillIn(object parent, MemberInfo member) {
        SetMember(parent, member, RNG.Generation.Chance(0.5f));
    }
}
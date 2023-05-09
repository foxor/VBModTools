using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(8)]
public class SString : ISerializable, ICastable<string>, IGenerateable, IPoolable {
    [GenerateString]
    public string Value;
    public SString(string Value) {
        this.Value = Value;
        if (this.Value == null) {
            this.Value = "";
        }
    }
    public SString() : this("") {}
    public void DeSerialize(Stream stream) {
        Value = stream.ConsumeString();
    }
    public int SerializationSize() {
        return Stream.StringSize(Value);
    }

    public void Serialize(Stream stream) {
        stream.WriteString(Value);
    }

    public string Cast() {
        return Value;
    }
    public void Return() {
        Value = null;
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
        Value = null;
    }
    public static implicit operator string(SString b) {
        return b == null ? "" : b.Value;
    }
    public static implicit operator SString(string b) {
        return new SString(){Value = b};
    }
    public static implicit operator SString(char b) {
        return new SString(){Value = b.ToString()};
    }

    public override bool Equals(object obj) {
        if (Value == null) {
            return false;
        }
        return Cast().Equals(obj);
    }
    public override int GetHashCode() {
        return Cast().GetHashCode();
    }
    public override string ToString() {
        return Value;
    }
}

public class GenerateString : Generate {
    public override void FillIn(object obj, System.Reflection.MemberInfo memberInfo) {
        SetMember(obj, memberInfo, "");
    }
}
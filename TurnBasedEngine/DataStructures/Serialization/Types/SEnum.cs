using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(5)]
public class SEnum<T> : ISerializable, ICastable<T>, IGenerateable, IPoolable
    where T : System.Enum
{
    protected static T[] values;
    public static T[] Values {
        get {
            if (values == null) {
                var enumValues = System.Enum.GetValues(typeof(T));
                Assert.That(enumValues.Length < 255, "Enums must be backed by bytes, and 255 is reserved");
                values = new T[enumValues.Length];
                for (int i = 0; i < enumValues.Length; i++) {
                    // For some reason, Array.GetValue allocates, so cast this back to an actual array.
                    values[i] = (T)enumValues.GetValue(i);
                }
            }
            return values;
        }
    }
    protected static string[] stringValues;
    public static string[] StringValues {
        get {
            if (stringValues == null) {
                stringValues = Values.Select(x => x.ToString()).ToArray();
            }
            return stringValues;
        }
    }
    private byte value;
    public T Value {
        get {
            Assert.That(HasValue, $"Trying to get the value of an uninitialized SEnum<{typeof(T).GetNameCached()}>.");
            return Values[value];
        }
        set {
            this.value = (byte)((object)value);
        }
    }
    public SEnum() : this((byte)255) {}
    public SEnum(byte Value) {
        this.value = Value;
    }
    public SEnum(T Value) {
        // YEEEEE HAW!
        // Seriously though, enums are required to be backed by bytes
        this.value = (byte)((object)Value);
    }
    public void DeSerialize(Stream stream) {
        value = stream.ConsumeByte();
    }
    public int SerializationSize() {
        return 1;
    }

    public void Serialize(Stream stream) {
        stream.WriteByte(value);
    }
    public bool HasValue {
        get => value != 255;
    }
    public T Cast() {
        return Value;
    }
    public void Return() {
        value = 255;
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
        value = 255;
    }
    public static implicit operator T(SEnum<T> v) {
        if (v == null) {
            return default(T);
        }
        return v.Value;
    }
    public static implicit operator SEnum<T>(T v) {
        return new SEnum<T>(v);
    }
    public override bool Equals(object obj) {
        if (obj is SEnum<T> other) {
            return HasValue == other.HasValue && value == other.value;
        }
        if (HasValue) {
            return Value.Equals(obj);
        }
        return false;
    }
    public bool Equals(SEnum<T> other) {
        if (!HasValue || other == null || !other.HasValue) {
            return false;
        }
        return value == other.value;
    }
    public override int GetHashCode() {
        return value;
    }
    public override string ToString() {
        if (HasValue) {
            return Value.ToString();
        }
        else {
            return $"<Empty {typeof(T).GetNameCached()}>";
        }
    }
}
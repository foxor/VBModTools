using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Object = System.Object;

[TypeIndex(7)]
public class SInt2 : ISerializable, IGenerateable, IPoolable {
    // 1-7 is the non-wall area of a normal 9x9 map
    [GenerateInt(1, 7)]
    public int X { get; set; }
    [GenerateInt(1, 7)]
    public int Y { get; set; }
    public SInt2() : this(0, 0) { }
    public SInt2(int x, int y) {
        this.X = x;
        this.Y = y;
    }
    public void DeSerialize(Stream stream) {
        X = stream.ConsumeInt();
        Y = stream.ConsumeInt();
    }

    public int SerializationSize() {
        return Stream.IntSize() * 2;
    }

    public void Serialize(Stream stream) {
        stream.WriteInt(X);
        stream.WriteInt(Y);
    }

    public int Length {
        get {
            return Mathf.Abs(X) + Mathf.Abs(Y);
        }
    }

    // These are like this so these can behave like references for the purposes of dictionaries.
    // If you want to compare them numerically, use the == or != operator
    public override bool Equals(object obj) => Object.ReferenceEquals(this, obj);
    public override int GetHashCode() => base.GetHashCode();

    public int Dot(SInt2 other) {
        return X * other.X + Y * other.Y;
    }
    public bool IsNormalizable() {
        return (X == 0) != (Y == 0);
    }
    public CardinalDirection Normalize() {
        var mx = Mathf.Abs(X);
        var my = Mathf.Abs(Y);
        if (mx == my) {
            throw new Exception("Can't normalize diagonal vector");
        }
        if (mx > my) {
            if (X > 0) {
                return CardinalDirection.East;
            }
            return CardinalDirection.West;
        }
        if (Y > 0) {
            return CardinalDirection.North;
        }
        return CardinalDirection.South;
    }
    public void Return() {
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
    }

    public static implicit operator Vector3(SInt2 v) {
        Assert.That(v != null, $"Attempting to read from a vec2 that has never been written");
        return new Vector3(v.X, v.Y);
    }
    public static implicit operator SInt2(Vector3 v) {
        return new SInt2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }
    public static implicit operator CardinalDirection(SInt2 v) {
        Assert.That(v != null, $"Attempting to read from a vec2 that has never been written");
        return v.Normalize();
    }
    public static implicit operator SInt2(CardinalDirection v) {
        switch (v) {
            case CardinalDirection.North: {
                return new SInt2(0, 1);
            }
            case CardinalDirection.South: {
                return new SInt2(0, -1);
            }
            case CardinalDirection.East: {
                return new SInt2(1, 0);
            }
            case CardinalDirection.West: {
                return new SInt2(-1, 0);
            }
        }
        throw new Exception("Can't convert unexpected direction to vector");
    }
    public static implicit operator ulong(SInt2 a) {
        Assert.That(a != null, $"Attempting to read from a vec2 that has never been written");
        // The cast to uint is to avoid writing a bunch of 1s if it's negative.
        return ((ulong)a.X) << 32 | ((uint)a.Y);
    }
    public static implicit operator SInt2(ulong a) {
        return new SInt2((int)(a >> 32), (int)((uint)(a & 0xffffffff)));
    }

    public static SInt2 operator +(SInt2 a, SInt2 b) => new SInt2(a.X + b.X, a.Y + b.Y);
    public static SInt2 operator -(SInt2 a, SInt2 b) => new SInt2(a.X - b.X, a.Y - b.Y);
    public static SInt2 operator -(SInt2 a) => new SInt2(-a.X, -a.Y);
    public static SInt2 operator +(SInt2 a, CardinalDirection b) => a + b.ToSInt2();
    public static SInt2 operator *(SInt2 a, int b) => new SInt2(a.X * b, a.Y * b);
    public static Vector3 operator *(SInt2 a, float b) => ((Vector3)a) * b;
    public static Vector2 operator +(SInt2 a, Vector2 b) => new Vector2(a.X + b.x, a.Y + b.y);

    public static bool operator ==(SInt2 a, SInt2 b) {
        var aNull = Object.ReferenceEquals(a, null);
        var bNull = Object.ReferenceEquals(b, null);
        if (aNull && bNull) {
            return true;
        }
        if (aNull != bNull) {
            return false;
        }
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(SInt2 a, SInt2 b) {
        var aNull = Object.ReferenceEquals(a, null);
        var bNull = Object.ReferenceEquals(b, null);
        if (aNull && bNull) {
            return false;
        }
        if (aNull != bNull) {
            return true;
        }
        return a.X != b.X || a.Y != b.Y;
    }
    public override string ToString() {
        return $"{{{X}, {Y}}}";
    }
}
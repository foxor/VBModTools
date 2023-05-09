using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These are ordered such that adding one rotates left (right hand)
[SerializableEnum]
[TypeIndex(66)]
public enum CardinalDirection : byte {
    North,
    West,
    South,
    East,
}

[ProtectStatics]
public static class CardinalDirectionExtensions {
    public static readonly SInt2[] CardinalDirections = new SInt2[] {
        new SInt2( 0,  1),
        new SInt2(-1,  0),
        new SInt2( 0, -1),
        new SInt2( 1,  0),
    };
    public static readonly SInt2[] AllDirections = new SInt2[] {
        new SInt2(-1, -1),
        new SInt2(-1,  0),
        new SInt2(-1,  1),
        new SInt2( 0, -1),
        new SInt2( 0,  1),
        new SInt2( 1, -1),
        new SInt2( 1,  0),
        new SInt2( 1,  1)
    };
    public static SInt2 ToSInt2(this CardinalDirection direction) {
        switch (direction) {
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
        throw new System.Exception("Unsupported direction!");
    }

}
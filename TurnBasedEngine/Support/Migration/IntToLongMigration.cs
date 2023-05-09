using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntToLongSObjMigration : VersionUpgradeAttribute {
    public IntToLongSObjMigration(int Version, int Key) : base(Version, Key) {
    }

    public override void UpgradeProperty(object parent) {
        var oldValue = (parent as SObj).GetRaw(Key);
        if (oldValue as SInt is var oldInt && oldInt != null) {
            (parent as SObj)[Key] = new SLong((ulong)(oldInt.Value));
        }
    }
}
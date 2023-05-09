using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceWithNullMigration : VersionUpgradeAttribute {
    public ReplaceWithNullMigration(int Version, int Key) : base(Version, Key) {
    }
    public override void UpgradeProperty(object parent) {
        (parent as SObj)[Key] = null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurgeDataMigration : VersionUpgradeAttribute {
    public PurgeDataMigration(int Version, int Key) : base(Version, Key) {
    }

    public override void UpgradeProperty(object parent) {
        (parent as SObj)[Key] = new SNull();
    }
}
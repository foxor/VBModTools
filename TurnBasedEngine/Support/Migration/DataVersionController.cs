using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public abstract class VersionUpgradeAttribute : Attribute {
    public int UpgradeVersion;
    public int Key;
    public VersionUpgradeAttribute(int UpgradeVersion, int Key) {
        this.UpgradeVersion = UpgradeVersion;
        this.Key = Key;
    }
    public abstract void UpgradeProperty(object parent);
}

[ProtectStatics]
public static class DataVersionController {
    public static ushort CURRENT_DATA_VERSION = 11;
    public static bool MadeAnyChanges;

    public static void EnsureCurrent(object owner, PropertyInfo property, int? setArgument, int version) {
        while (version < CURRENT_DATA_VERSION) {
            version += 1;
            var attributes = property.GetCustomAttributesCached()
                .OfType<VersionUpgradeAttribute>()
                .Where(x => x.UpgradeVersion == version);
            foreach (var upgradeAttribute in attributes) {
                upgradeAttribute.UpgradeProperty(owner);
                Debug.Log($"Upgrading version to {version} by invoking {upgradeAttribute.GetType()} on {owner}");
                MadeAnyChanges = true;
            }
        }
    }
}
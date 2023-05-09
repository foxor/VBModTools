using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

[Serializable]
// This is kinda junk.  It needs an editor, and the backing type should get switched to the index
public class SerializedType : IEquatable<SerializedType> {
    [SerializeField]
    private string BackingData;

    private Type type;
    public Type Type {
        get {
            if (type == null && !string.IsNullOrEmpty(BackingData)) {
                var matchingTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => string.Equals(x.GetNameCached(), BackingData));
                Assert.That(matchingTypes.Any(), $"Unable to find a type for {BackingData}");
                type = matchingTypes.Single();
            }
            return type;
        }
        set {
            type = value;
            BackingData = type.GetNameCached();
        }
    }

    public override string ToString() {
        return BackingData;
    }

    public override int GetHashCode() {
        return BackingData.GetHashCode();
    }

    public bool Equals(SerializedType other) {
        return Type == other.Type;
    }

    public static implicit operator Type(SerializedType b) {
        return b.Type;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeIndex(3)]
public class SNull : ISerializable, IGenerateable {
    public void DeSerialize(Stream stream) {
    }

    public int SerializationSize() {
        return 0;
    }

    public void Serialize(Stream stream) {
    }
    public static bool IsNull(object obj) {
        return object.ReferenceEquals(obj, null) || obj.GetType() == typeof(SNull);
    }
}
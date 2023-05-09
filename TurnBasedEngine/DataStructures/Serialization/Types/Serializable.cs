using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[TypeIndex(41)]
public interface ISerializable : IAssemblyType {
    void DeSerialize(Stream stream);
    int SerializationSize();
    void Serialize(Stream stream);
}

public static class SerializableExtensions {
    public static Stream ToStream(this ISerializable serializable) {
        var stream = new Stream(Stream.SerializationSize(serializable));
        stream.Write(serializable);
        // reset index for reading;
        stream.index = 0;
        return stream;
    }
    public static T DeepCopy<T>(this T originalCopy) where T : ISerializable {
        return originalCopy.ToStream().Consume<T>();
    }
}

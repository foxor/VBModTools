using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Interface)]
public class TypeIndexAttribute : Attribute {
    public ushort Index;
    public TypeIndexAttribute(ushort Index) {
        this.Index = Index;
    }
}

[ProtectStatics]
public static class TypeIndexExtension {
    private static Dictionary<Type, ushort> TypeIndicies = new Dictionary<Type, ushort>();
    public static bool HasIndex(this Type type) {
        return type.GetCustomAttributesCached().OfType<TypeIndexAttribute>().Any();
    }
    public static ushort GetIndex(this Type type) {
        if (!TypeIndicies.TryGetValue(type, out var index)) {
            var sourcedIndex = type.GetCustomAttributesCached()?.OfType<TypeIndexAttribute>()?.SingleOrDefault()?.Index;
            if (sourcedIndex.HasValue) {
                TypeIndicies[type] = index = sourcedIndex.Value;
            }
            else {
                throw new Exception($"Type {type.GetNameCached()} has no type index!");
            }
        }
        return index;
    }
}

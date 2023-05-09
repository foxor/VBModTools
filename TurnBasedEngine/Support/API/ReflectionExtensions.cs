using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public class CreatedTypeCachedValue {
    public Type[] typeParams;
    public Type createdType;
    public CreatedTypeCachedValue(Type genericType, Type[] typeParams) {
        var paramLength = typeParams.Length;
        this.typeParams = new Type[paramLength];
        for (int i = paramLength - 1; i >= 0; i--) {
            this.typeParams[i] = typeParams[i];
        }
        createdType = genericType.MakeGenericType(this.typeParams);
    }
    public bool Match(Type[] testParams) {
        if (testParams.Length != typeParams.Length) {
            return false;
        }
        var match = true;
        for (int i = testParams.Length - 1; i >= 0; i--) {
            match &= testParams[i] == typeParams[i];
        }
        return match;
    }
}

[ProtectStatics]
public static class ReflectionExtensions {
    public static Dictionary<Type, bool> ContainsGenericParameterCache = new Dictionary<Type, bool>();
    public static bool ContainsGenericParametersCached(this Type t) {
        bool answer = false;
        if (!ContainsGenericParameterCache.TryGetValue(t, out answer)) {
            answer = t.ContainsGenericParameters;
            ContainsGenericParameterCache[t] = answer;
        }
        return answer;
    }

    public static Dictionary<Type, List<CreatedTypeCachedValue>> CreatedTypeCache = new Dictionary<Type, List<CreatedTypeCachedValue>>(); 
    public static Type MakeGenericTypeCached(this Type t, Type[] typeParams) {
        List<CreatedTypeCachedValue> cache = null;
        if (!CreatedTypeCache.TryGetValue(t, out cache)) {
            cache = new List<CreatedTypeCachedValue>();
            CreatedTypeCache[t] = cache;
        }
        foreach (var cachedValue in cache) {
            if (cachedValue.Match(typeParams)) {
                return cachedValue.createdType;
            }
        }
        var newCachedValue = new CreatedTypeCachedValue(t, typeParams);
        cache.Add(newCachedValue);
        return newCachedValue.createdType;
    }

    public static Dictionary<PropertyInfo, Attribute[]> CustomAttributeCache = new Dictionary<PropertyInfo, Attribute[]>();
    public static Attribute[] GetCustomAttributesCached(this PropertyInfo property) {
        if (!CustomAttributeCache.TryGetValue(property, out var r)) {
            CustomAttributeCache[property] = r = property.GetCustomAttributes().ToArray();
        }
        return r;
    }
    public static Attribute GetCustomAttributeCached(this PropertyInfo property, Type attributeType) {
        return property.GetCustomAttributesCached().Where(x => attributeType.IsAssignableFrom(x.GetType())).SingleOrDefault();
    }
    public static Dictionary<Type, Attribute[]> TypeAttributeCache = new Dictionary<Type, Attribute[]>();
    public static Attribute[] GetCustomAttributesCached(this Type type) {
        if (!TypeAttributeCache.TryGetValue(type, out var r)) {
            var inherit = false;
            TypeAttributeCache[type] = r = type.GetCustomAttributes(inherit).OfType<Attribute>().ToArray();
        }
        return r;
    }
    public static Attribute GetCustomAttributeCached(this Type type, Type attributeType) {
        return type.GetCustomAttributesCached().Where(x => attributeType.IsAssignableFrom(x.GetType())).SingleOrDefault();
    }
    public static Dictionary<Type, Dictionary<BindingFlags, PropertyInfo[]>> TypePropertyCache = new Dictionary<Type, Dictionary<BindingFlags, PropertyInfo[]>>();
    public static PropertyInfo[] GetPropertiesCached(this Type type, BindingFlags flags) {
        if (!TypePropertyCache.TryGetValue(type, out var flagToProperties)) {
            flagToProperties = new Dictionary<BindingFlags, PropertyInfo[]>();
            TypePropertyCache[type] = flagToProperties;
        }
        if (!flagToProperties.TryGetValue(flags, out var properties)) {
            properties = type.GetProperties(flags);
            flagToProperties[flags] = properties;
        }
        return properties;
    }
    public static PropertyInfo[] GetPropertiesCached(this Type type) {
        // This is the value of Type.DefaultLookup in mscorelib
        return GetPropertiesCached(type, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
    }
    public static Dictionary<ConstructorInfo, object[]> ConstructorParameterCache = new Dictionary<ConstructorInfo, object[]>();
    public static object[] GetParamterDefaultValuesCached(this ConstructorInfo constructor) {
        if (!ConstructorParameterCache.TryGetValue(constructor, out var r)) {
            ConstructorParameterCache[constructor] = r = constructor.GetParameters().Select(x => x.DefaultValue).ToArray();
        }
        return r;
    }
    public static Dictionary<MemberInfo, string> NameCache = new Dictionary<MemberInfo, string>();
    public static string GetNameCached(this MemberInfo type) {
        if (!NameCache.TryGetValue(type, out var r)) {
            NameCache[type] = r = type.Name;
        }
        return r;
    }
}

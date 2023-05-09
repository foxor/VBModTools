using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public static class Constructor {
    public static Dictionary<Type, ConstructorInfo> _defaultConstructors;
    public static Dictionary<Type, ConstructorInfo> DefaultConstructors {
        get {
            if (_defaultConstructors == null) {
                _defaultConstructors = new Dictionary<Type, ConstructorInfo>();
            }
            return _defaultConstructors;
        }
    }
    public static Transform _defaultViewParent;
    public static Transform DefaultViewParent {
        get {
            if (_defaultViewParent == null) {
                var canvas = GameObject.FindGameObjectWithTag("MainCanvas");
                if (canvas != null) {
                    _defaultViewParent = canvas.transform;
                }
            }
            return _defaultViewParent;
        }
    }
    public static bool IsConstructableType(Type T) {
        return !T.IsAbstract && !T.IsInterface && !T.IsGenericTypeDefinition && !T.IsGenericParameter;
    }
    public static bool IsRecursivlyConstructableType(Type T) {
        return IsConstructableType(T) && T.GetGenericArguments().All(IsRecursivlyConstructableType);
    }
    public static ConstructorInfo SmallestDefaultValuedConstructor(Type T) {
        ConstructorInfo constructor = null;
        if (DefaultConstructors.TryGetValue(T, out constructor)) {
            return constructor;
        }
        if (!IsConstructableType(T)) {
            throw new Exception($"Can't construct instance of type: {T}");
        }
        var constructors = T.GetConstructors(
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.CreateInstance |
                BindingFlags.Instance)
            .OrderBy(c => c.GetParameters().Count())
            .Where(c => c.GetParameters().All(p => p.HasDefaultValue));
        if (constructors.Any()) {
            constructor = constructors.First();
            DefaultConstructors[T] = constructor;
            return constructor;
        }
        throw new Exception("Unable to find default valued constructor for type " + T.FullName);
    }
    public static T Construct<T>() {
        return (T)Construct(typeof(T));
    }
    public static object Construct(Type T) {
        var pooled = Pool.CheckOut(T);
        if (!ReferenceEquals(pooled, null)) {
            return pooled;
        }
        if (typeof(ScriptableObject).IsAssignableFrom(T)) {
            return ScriptableObject.CreateInstance(T);
        }
        if (typeof(IView).IsAssignableFrom(T) && !typeof(ILatentView).IsAssignableFrom(T)) {
            var prefab = ViewMap.Instance[T];
            Assert.That(!SNull.IsNull(prefab), "Missing viewmap prefab for view");
            var viewGameObject = GameObject.Instantiate(prefab, DefaultViewParent);
            return viewGameObject.GetComponent(T);
        }
        if (T.IsEnum) {
            var sEnumType = typeof(SEnum<>).MakeGenericTypeCached(new Type[] { T });
            var values = (Array)sEnumType.GetProperty("Values").GetMethod.Invoke(null, null);
            return values.GetValue(RNG.Generation.Roll(values.Length) - 1);
        }
        try {
            var constructor = SmallestDefaultValuedConstructor(T);
            if (constructor == null) {
                return null;
            }
            var parameters = constructor.GetParamterDefaultValuesCached();
            return constructor.Invoke(parameters);
        }
        catch (Exception e) {
            throw e;
        }
    }
}
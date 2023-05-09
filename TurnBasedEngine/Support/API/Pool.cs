using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable : IDisposable {
    void Return();
}

public class Pool {
    protected static Dictionary<Type, List<object>> pooledValues;
    protected static Dictionary<Type, List<object>> PooledValues {
        get {
            if (pooledValues == null) {
                pooledValues = new Dictionary<Type, List<object>>();
            }
            return pooledValues;
        }
    }

    public static int PoolSize(Type type) {
        return 30;
    }

    public static object CheckOut(Type type) {
        if (!PooledValues.TryGetValue(type, out var pool)) {
            return null;
        }
        while (pool.Count > 0) {
            var index = pool.Count() - 1;
            var elem = pool[index];
            pool.RemoveAt(index);
            if (elem == null) {
                continue;
            }
            if (elem is MonoBehaviour mono) {
                if (mono == null || mono.gameObject == null) {
                    continue;
                }
            }
            return elem;
        }
        return null;
    }

    public static bool Return(object toReturn) {
        var type = toReturn.GetType();
        List<object> pool = null;
        if (!PooledValues.TryGetValue(type, out pool)) {
            pool = new List<object>();
            PooledValues[type] = pool;
        }
        if (pool.Count() >= PoolSize(type)) {
            (toReturn as IDisposable)?.Dispose();
            Debug.LogWarning($"Exceeded availale pool size for {type}.  Increase the pool size?");
            return false;
        }
        Assert.That(!pool.Any(x => ReferenceEquals(x, toReturn)), "Attempted to double return!");
        pool.Add(toReturn);
        return true;
    }
}
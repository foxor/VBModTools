using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayPool<T> {
    protected static List<List<T[]>> pooledValues;
    protected static List<List<T[]>> PooledValues {
        get {
            if (pooledValues == null) {
                pooledValues = new List<List<T[]>>();
            }
            return pooledValues;
        }
    }

    public static T[] Get(int length) {
        for (int i = length - PooledValues.Count + 1; i >= 0; i--) {
            PooledValues.Add(new List<T[]>());
        }
        var poolOfLength = PooledValues[length];
        if (poolOfLength.Any()) {
            var lastIndex = poolOfLength.Count - 1;
            var last = poolOfLength[lastIndex];
            poolOfLength.RemoveAt(lastIndex);
            return last;
        }
        return new T[length];
    }

    protected static int PoolSizePerLength() {
        return 4;
    }

    public static void Return(T[] array, bool clear = true) {
        int length = array.Length;
        for (int i = length - PooledValues.Count + 1; i >= 0; i--) {
            PooledValues.Add(new List<T[]>());
        }
        var poolOfLength = PooledValues[length];
        if (poolOfLength.Count < PoolSizePerLength()) {
            poolOfLength.Add(array);
        }
    }
}

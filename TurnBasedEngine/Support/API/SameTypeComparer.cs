using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameTypeEquality<T> : IEqualityComparer<T> {
    bool IEqualityComparer<T>.Equals(T x, T y) {
        return x.GetType() == y.GetType();
    }
    int IEqualityComparer<T>.GetHashCode(T obj) {
        return obj.GetType().GetHashCode();
    }
}
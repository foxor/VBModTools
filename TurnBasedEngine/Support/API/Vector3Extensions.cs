using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions {
    public static bool Approximately(this Vector3 a, Vector3 b) {
        return Mathf.Approximately(a.x, b.x) &&
            Mathf.Approximately(a.y, b.y) &&
            Mathf.Approximately(a.z, b.z);
    }
}
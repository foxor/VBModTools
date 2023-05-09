using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializationPerformanceMonitor : MonoBehaviour {
    protected static SerializationPerformanceMonitor instance;
    public static SerializationPerformanceMonitor Instance {
        get {
            if (instance == null && !TestDetector.AreTestsActive) {
                instance = new GameObject("SerializationPerformanceMonitorGO", typeof(SerializationPerformanceMonitor)).GetComponent<SerializationPerformanceMonitor>();
                instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            return instance;
        }
    }

    protected bool ignoreFrame;
    public static void IgnoreFrame() {
        Instance.ignoreFrame = true;
    }
    [Conditional("ASSERTS_ENABLED")]
    public static void Warn(float totalMilliseconds) {
        // During loading, there's going to be a bunch of serialization.
        if (Time.timeSinceLevelLoad > 3 && totalMilliseconds > 1f/24f) {
            UnityEngine.Debug.LogWarning($"Missed 24fps framerate!");
            Instance.ignoreFrame = true;
        }
    }
    public void Update() {
        var totalMilliseconds = Time.deltaTime;
        if (ignoreFrame) {
            ignoreFrame = false;
            return;
        }
        Warn(totalMilliseconds);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneController {
    public static IDisposable ActiveModel;
    public static event Action OnSceneChanged;
    public static M LoadScene<M>() where M : Model {
        return (M)LoadScene(typeof(M));
    }
    public static Model LoadScene(Type M) {
        CoroutineRunner.StopCoroutines();
        if (ActiveModel != null) {
            ActiveModel.Dispose();
        }
        if (OnSceneChanged != null) {
            OnSceneChanged();
        }
        // Clean up whatever random statics are left, including ActiveModel and any orphans
        // This clears our OnSceneChanged as well
        StaticResetTool.Reset();
        var model = Constructor.Construct(M) as Model;
        model.Setup();
        ActiveModel = model;
        SerializationPerformanceMonitor.IgnoreFrame();
        return model;
    }
}
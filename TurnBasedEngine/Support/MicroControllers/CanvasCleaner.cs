using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCleaner : MonoBehaviour {
    protected bool mustReregister = true;
    public void Update() {
        if (mustReregister) {
            SceneController.OnSceneChanged += Cleanup;
            mustReregister = false;
        }
    }

    protected void Cleanup() {
        transform.DestroyAllChildren();
        mustReregister = true;
    }
}
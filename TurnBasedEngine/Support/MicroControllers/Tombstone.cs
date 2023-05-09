using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a place to put dead things you need for later.
public class Tombstone : MonoBehaviour {
    protected static Tombstone instance;
    protected static bool instanciating;
    public static Tombstone Instance {
        get {
            if (instance == null) {
                instanciating = true;
                instance = new GameObject("Tombstone", typeof(Tombstone)).GetComponent<Tombstone>();
                instanciating = false;
            }
            return instance;
        }
    }

    protected void Awake() {
        if (!instanciating) {
            Destroy(gameObject);
            return;
        }
        SceneController.OnSceneChanged += () => { Destroy(gameObject); };
        gameObject.SetActive(false);
    }
}
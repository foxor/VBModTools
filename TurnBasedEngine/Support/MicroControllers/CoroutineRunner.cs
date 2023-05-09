using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtectStatics]
public class CoroutineRunner : MonoBehaviour {
    protected static CoroutineRunner instance;
    protected static CoroutineRunner Instance {
        get {
            if (instance == null) {
                instance = new GameObject("CoroutineRunner", typeof(CoroutineRunner)).GetComponent<CoroutineRunner>();
                GameObject.DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
    protected static List<Coroutine> activeCoroutines;
    protected static List<Coroutine> ActiveCoroutines {
        get {
            if (activeCoroutines == null) {
                activeCoroutines = new List<Coroutine>();
            }
            return activeCoroutines;
        }
    }
    public static void Run(IEnumerator coroutine, Action onComplete = null) {
        if (Application.isPlaying) {
            ActiveCoroutines.Add(Instance.StartCoroutine(Instance.RunInternal(coroutine, onComplete)));
        }
        else {
            int i = 0;
            for (i = 0; i < 10000 && coroutine != null && coroutine.MoveNext(); i++);
            if (i >= 10000) {
                Debug.LogWarning("Coroutine loop hitting iteration limit in editor");
            }
            onComplete();
        }
    }

    protected IEnumerator RunInternal(IEnumerator coroutine, Action onComplete) {
        // If this is a 0-frame coroutine (most are), callback in this stack frame
        while (coroutine?.MoveNext() == true) {
            yield return coroutine.Current;
        }
        if (onComplete != null) {
            onComplete();
        }
    }

    public static void StopCoroutines() {
        Instance.StopAllCoroutines();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IFocusable {
    void OnFocusBegin();
    void OnFocusEnd();
}

public class RaycastController : MonoBehaviour {
    protected static RaycastController instance;
    protected static bool instanciating;
    public static RaycastController Instance {
        get {
            if (instance == null) {
                instanciating = true;
                instance = new GameObject("Raycast Controller", typeof(RaycastController)).GetComponent<RaycastController>();
                instanciating = false;
            }
            return instance;
        }
    }

    protected class FocusableStatus : IPoolable {
        public IFocusable subject;
        public bool parity;
        void IPoolable.Return() { }
        void IDisposable.Dispose() { }
    }

    protected Dictionary<Transform, IFocusable> focusableCache = new Dictionary<Transform, IFocusable>();
    protected Dictionary<IFocusable, IFocusable> focusableParents = new Dictionary<IFocusable, IFocusable>();
    protected List<FocusableStatus> activeFocus = new List<FocusableStatus>();
    protected bool currentParity;
    protected PointerEventData pointerData;
    protected List<RaycastResult> raycastResults = new List<RaycastResult>();

    public IEnumerable<IFocusable> ActiveFocus {
        get {
            return activeFocus.Select(x => x.subject);
        }
    }

    protected void Awake() {
        if (!instanciating) {
            Destroy(gameObject);
            return;
        }
        pointerData = new PointerEventData(FindObjectOfType<EventSystem>());
        SceneController.OnSceneChanged += () => { Destroy(gameObject); };
    }

    protected IEnumerable<IFocusable> FindRelatedFocusables(GameObject gameObject) {
        return gameObject.GetComponentsInParent<IFocusable>();
    }

    public void Update() {
        if (LockController.IsBusy) {
            return;
        }
        currentParity ^= true;
        pointerData.position = Input.mousePosition;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        if (Input.GetKeyDown(KeyCode.R) && Application.isEditor) {
            foreach (var result in raycastResults) {
                Debug.Log($"Hit {result.gameObject.name}");
            }
        }
        foreach (var raycastResult in raycastResults) {
            var related = FindRelatedFocusables(raycastResult.gameObject).ToArray();
            foreach (var focusable in related) {
                var correspondingIndex = activeFocus.IndexOf(x => x.subject == focusable);
                if (correspondingIndex == -1) {
                    correspondingIndex = activeFocus.Count;
                    var focus = Constructor.Construct<FocusableStatus>();
                    focus.subject = focusable;
                    activeFocus.Add(focus);
                    focusable.OnFocusBegin();
                }
                activeFocus[correspondingIndex].parity = currentParity;
            }
            if (related.Any()) {
                break;
            }
        }
        var lostFocus = activeFocus.Where(x => x.parity != currentParity);
        foreach (var focus in lostFocus) {
            focus.subject.OnFocusEnd();
            Pool.Return(focus);
        }
        activeFocus.RemoveAll(lostFocus.Contains);
    }
    // A NOP to allow RaycastController.Instance.Something()
    public void EnsureCreated() {
    }
}
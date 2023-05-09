using UnityEngine;
#if PLATFORM_STANDALONE_WIN && !UNITY_EDITOR
using ResizeHandler;
#endif

public class WindowController : MonoBehaviour {
    public float aspect;
#if PLATFORM_STANDALONE_WIN && !UNITY_EDITOR
    void Start() {
        ResizeHandler.AspectController.EnforceAspect(aspect);
        Application.wantsToQuit += ApplicationWantsToQuit;
    }

    private bool ApplicationWantsToQuit() {
        ResizeHandler.AspectController.OnExit();
        return true;
    }
#endif
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ITooltipParent<T> where T : IEnumerable<Model> {
    T TooltipChildren { get; set; }
}
public static class TooltipParentExtensions {
    public static ulong ChildRelationship<T>(this ITooltipParent<T> log) where T : IEnumerable<Model> {
        if (log is Model model) {
            var tooltipChildren = log.TooltipChildren;
            if (tooltipChildren is LogStack logStack) {
                return logStack.RootRelationship;
            }
            var index = model.innerList.IndexOf(tooltipChildren as ISerializable);
            return new SInt2(model.Id, index);
        }
        else if (log is LogStack logStack) {
            return logStack.RootRelationship;
        }
        throw new System.Exception("Unsuported Tooltip parent type");
    }
}
public abstract class TooltipView<M, P> : View<M>, IFocusable where M : Model, ITooltipParent<P> where P : IEnumerable<Model> {
    public Canvas TooltipCanvas;
    public RectTransform TooltipParent;
    public bool MoveTooltip = true;
    private bool hasActiveParent;
    private bool HasActiveParent { get => hasActiveParent; set { hasActiveParent = value; CheckFirstDisplayed(); } }
    private bool hasBeenSetup;
    private bool HasBeenSetup { get => hasBeenSetup; set { hasBeenSetup = value; CheckFirstDisplayed(); } }
    private bool displayed;
    private bool focused;
    private ulong childRelationship;
    public virtual ulong GetParentRelationship(M tooltipped) => ulong.MaxValue;
    public override void Setup(in M tooltipped) {
        base.Setup(tooltipped);
        var parentRelationship = GetParentRelationship(tooltipped);
        if (parentRelationship != ulong.MaxValue) {
            HasActiveParent |= TransformMatchmaker.FindParent(parentRelationship, transform);
        }
        RaycastController.Instance.EnsureCreated();
        childRelationship = tooltipped.ChildRelationship();
        TransformMatchmaker.RegisterParent(childRelationship, TooltipParent.transform);
        HasBeenSetup = true;
    }
    private void CheckFirstDisplayed() {
        if (HasActiveParent && HasBeenSetup && !displayed) {
            displayed = true;
            OnFirstDisplayed();
        }
    }
    protected virtual void OnFirstDisplayed() {
        UpdateCanvasOrder();
        if (TooltipCanvas.gameObject.activeInHierarchy) {
            OnFocusBegin();
        }
    }
    public virtual void OnEnable() {
        HasActiveParent |= true;
    }
    protected void UpdateCanvasOrder() {
        var parentCanvas = TooltipCanvas.transform.parent.GetComponentInParent<Canvas>();
        TooltipCanvas.sortingOrder = parentCanvas == null ? 1 : parentCanvas.sortingOrder + 1;
    }
    public override void Teardown() {
        if (hasBeenSetup) {
            TransformMatchmaker.UnregisterParent(childRelationship, TooltipParent.transform);
        }
        if (focused) {
            LatentViewManager.Collapse(childRelationship);
        }
        hasActiveParent = false;
        hasBeenSetup = false;
        displayed = false;
        focused = false;
        childRelationship = ulong.MaxValue;
        base.Teardown();
    }
    public virtual void OnFocusBegin() {
        focused = true;
        LatentViewManager.Expand(childRelationship, TooltipParent.transform);
        if (TooltipParent.childCount == 0) {
            return;
        }
        TooltipCanvas.gameObject.SetActive(true);
        UpdateCanvasOrder();

        // TODO: this is a short-term solution.  Eventually, different configurations need to be able to specify different directions to pivot.
        if (!MoveTooltip) {
            return;
        }

        // Start in the default orientation
        TooltipCanvas.transform.SetAsLastSibling();
        TooltipParent.pivot = new Vector2(0f, 0.5f);
        TooltipParent.localPosition = new Vector3();

        StartCoroutine(Reposition());
    }

    public IEnumerator Reposition() {
        // I hate stutter frames, but can't figure out how to make the canvas re-flow correctly synchronously
        // This should work:
        //LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipCanvas.transform.parent as RectTransform);
        // but doesn't for reasons that are difficult to understand
        yield return null;

        // Measure where we are
        var corners = new Vector3[4];
        TooltipParent.GetWorldCorners(corners);

        var overRHS = corners[2].x > Screen.width;
        if (overRHS) {
            TooltipCanvas.transform.SetAsFirstSibling();
            TooltipParent.pivot = new Vector2(1f, 0.5f);
            TooltipParent.localPosition = new Vector3();
        }

        var bottomOverlap = -corners[0].y;
        var topOverlap = corners[1].y - Screen.height;
        if (bottomOverlap > 0) {
            TooltipParent.localPosition = new Vector3(0f, bottomOverlap);
        }
        else if (topOverlap > 0) {
            TooltipParent.localPosition = new Vector3(0f, -topOverlap);
        }
    }
    public virtual void OnFocusEnd() {
        if (this == null) {
            return;
        }
        TooltipCanvas.gameObject.SetActive(false);
    }
}

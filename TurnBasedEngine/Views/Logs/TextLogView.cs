using System.Linq;
using UnityEngine;
using TMPro;

public class TextLogView : TooltipView<TextLog, SList<Log>> {
    public Color FocusColor;
    public Color NormalColor;
    public TMP_Text TextField;
    protected TextLogView[] SplitViews;
    protected bool hasChildren;
    public override void Setup(in TextLog log) {
        base.Setup(log);
        IsTransformReady = TransformMatchmaker.FindParent(log.ModelRelationship, transform);
        TextField.fontStyle = FontStyles.Normal;
        Register<SString>(3, log, OnContentsUpdated);
        Register<SList<Log>>(1, log, OnTooltipChildrenUpdated);
    }
    public override void OnEnable() {
        IsTransformReady = true;
        base.OnEnable();
    }
    protected ILock OnContentsUpdated(in SString contents) {
        Contents = contents;
        return null;
    }
    protected ILock OnTooltipChildrenUpdated(in SList<Log> children) {
        hasChildren = children.Any();
        TextField.fontStyle = hasChildren ? FontStyles.Bold : FontStyles.Normal;
        return null;
    }
    protected bool isTransformReady = false;
    protected bool IsTransformReady { get => isTransformReady; set { isTransformReady = value; CheckSplit(); } }
    protected string contents = null;
    protected string Contents { get => contents; set { contents = value; CheckSplit(); } }
    protected bool initialized = false;
    protected void CheckSplit() {
        if (!IsTransformReady || Contents == null || initialized) {
            return;
        }
        initialized = true;
        var entries = Contents.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (!entries.Any()) {
            TextField.text = "";
            return;
        }
        TextField.text = entries[0];
        var parent = transform.parent;
        SplitViews = new TextLogView[entries.Length];
        for (int i = 1; i < entries.Length; i++) {
            // This kinda sucks, but TextLayoutGroups don't nest well with each other or text flow groups, including text objects themselves.
            // Layout groups only operate on rectangles, so we need the smallest rectangles so we can adjoin them.
            var copy = Constructor.Construct<TextLogView>();
            copy.transform.SetParent(parent);
            copy.transform.SetSiblingIndex(transform.GetSiblingIndex() + i);
            copy.TextField.text = entries[i];
            SplitViews[i] = copy;
        }
    }
    public override void OnFocusBegin() {
        if (hasChildren) {
            TextField.color = FocusColor;
        }
        base.OnFocusBegin();
    }
    public override void OnFocusEnd() {
        base.OnFocusEnd();
        TextField.color = NormalColor;
    }
    public override void Teardown() {
        if (SplitViews != null) {
            foreach (var sublog in SplitViews) {
                if (sublog != null) {
                    sublog.Return();
                }
            }
        }
        SplitViews = null;
        isTransformReady = false;
        initialized = false;
        contents = null;
        hasChildren = false;
        TextField.fontStyle = FontStyles.Normal;
        TextField.text = "";
        base.Teardown();
    }
}
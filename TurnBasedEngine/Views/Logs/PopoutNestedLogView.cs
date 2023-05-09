using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopoutNestedLogView : TooltipView<PopoutNestedLog, SList<Log>> {
    public RectTransform ToplineParent;
    protected bool Expanded = false;
    private ulong headlineRelationship;

    public override void Setup(in PopoutNestedLog log) {
        headlineRelationship = new SInt2(log.Id, 3);
        TransformMatchmaker.RegisterParent(headlineRelationship, ToplineParent);
        base.Setup(log);
    }
    public override ulong GetParentRelationship(PopoutNestedLog tooltipped) => tooltipped.ModelRelationship;
    protected override void OnFirstDisplayed() {
        LatentViewManager.Expand(headlineRelationship, ToplineParent);
        Expanded = true;
    }
    public override void Teardown() {
        TransformMatchmaker.UnregisterParent(headlineRelationship, ToplineParent);
        if (Expanded) {
            LatentViewManager.Collapse(headlineRelationship);
        }
        Expanded = false;
        base.Teardown();
    }
}
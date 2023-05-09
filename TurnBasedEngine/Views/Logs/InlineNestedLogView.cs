using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InlineNestedLogView : View<InlineNestedLog> {
    public Transform SublogParent;
    protected bool hasActiveTransform;
    protected bool hasModel;
    protected bool expanded;
    protected ulong childRelationship;

    public override void Setup(in InlineNestedLog log) {
        base.Setup(log);
        hasModel = true;
        childRelationship = log.ChildRelationship();
        hasActiveTransform |= TransformMatchmaker.FindParent(log.ModelRelationship, transform);
        TransformMatchmaker.RegisterParent(childRelationship, SublogParent);
        if (hasActiveTransform) {
            Expand();
        }
    }
    protected void OnEnable() {
        hasActiveTransform = true;
        if (hasModel) {
            Expand();
        }
    }
    protected void Expand() {
        expanded = true;
        LatentViewManager.Expand(childRelationship, SublogParent);
    }
    public override void Teardown() {
        if (expanded) {
            LatentViewManager.Collapse(childRelationship);
        }
        if (hasModel) {
            TransformMatchmaker.UnregisterParent(childRelationship, SublogParent);
        }
        hasActiveTransform = false;
        hasModel = false;
        childRelationship = ulong.MaxValue;
        base.Teardown();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ILatent {
    SLong ModelRelationship { get; set; }
}

public interface ILatentView {
    void Expand();
    void Collapse();
}

public class LatentViewManager {
    protected static Dictionary<ulong, List<ILatentView>> latentViews;
    protected static Dictionary<ulong, List<ILatentView>> LatentViews {
        get {
            if (latentViews == null) {
                latentViews = new Dictionary<ulong, List<ILatentView>>();
            }
            return latentViews;
        }
    }
    protected static Dictionary<ulong, List<ILatentView>> expandedViews;
    protected static Dictionary<ulong, List<ILatentView>> ExpandedViews {
        get {
            if (expandedViews == null) {
                expandedViews = new Dictionary<ulong, List<ILatentView>>();
            }
            return expandedViews;
        }
    }
    protected static HashSet<ulong> expandedRelationships;
    protected static HashSet<ulong> ExpandedRelationships {
        get {
            if (expandedRelationships == null) {
                expandedRelationships = new HashSet<ulong>();
            }
            return expandedRelationships;
        }
    }
    internal static void RegisterLatent(ulong relationship, ILatentView view) {
        if (ExpandedRelationships.Contains(relationship)) {
            view.Expand();
            if (!ExpandedViews.TryGetValue(relationship, out var expandedList)) {
                expandedList = new List<ILatentView>();
                ExpandedViews[relationship] = expandedList;
            }
            expandedList.Add(view);
            return;
        }
        if (!LatentViews.TryGetValue(relationship, out var latentList)) {
            latentList = new List<ILatentView>();
            LatentViews[relationship] = latentList;
        }
        latentList.Add(view);
    }
    internal static void UnregisterLatent(ulong relationship, ILatentView view) {
        if (LatentViews.TryGetValue(relationship, out var latentList)) {
            latentList.Remove(view);
        }
    }
    public static bool AnyLatentChildren(ulong relationship) {
        return LatentViews.Get(relationship)?.Any() == true;
    }
    public static void Expand(ulong relationship, Transform transform) {
        if (transform != null) {
            TransformMatchmaker.PrioritizeParent(relationship, transform);
        }
        // Add to the expanded set first in case the exchange causes new latents to register
        ExpandedRelationships.Add(relationship);
        if (LatentViews.TryGetValue(relationship, out var list)) {
            foreach (var view in list) {
                view.Expand();
            }
            ExpandedViews[relationship] = list;
            LatentViews.Remove(relationship);
        }
    }
    public static void Collapse(ulong relationship) {
        // Remove from the expanded set first in case the exchange causes new latents to register
        ExpandedRelationships.Remove(relationship);
        if (ExpandedViews.TryGetValue(relationship, out var list)) {
            foreach (var view in list) {
                view.Collapse();
            }
            LatentViews[relationship] = list;
            ExpandedViews.Remove(relationship);
        }
    }
}

// This is a super lightweight view that inflates into the real view only when necessary
public class LatentView<V,M> : IModelView<M>, IPoolable, ILatentView where V : IModelView<M> where M : Model, ILatent {
    protected V expandedView;
    protected int? modelId;
    protected ulong? modelRelationship;
    public void Setup(in M model) {
        modelId = model.Id;
        modelRelationship = model.ModelRelationship;
        LatentViewManager.RegisterLatent(model.ModelRelationship, this);
    }
    public void Expand() {
        var model = ModelViewController.Instance.GetViewModel(modelId.Value);
        Assert.That(model is M);
        ModelViewController.Instance.CreateView<V, M>((M)model, (v) => { expandedView = v; });
    }
    public void Collapse() {
        if (expandedView != null) {
            var model = ModelViewController.Instance.GetViewModel(modelId.Value);
            ModelViewController.Instance.RemoveFilteredViews(model, expandedView.Equals);
        }
    }

    void IPoolable.Return() {
        ((IDisposable)this).Dispose();
        Pool.Return(this);
    }
    void IDisposable.Dispose() {
        Collapse();
        expandedView = default;
        if (modelRelationship.HasValue) {
            LatentViewManager.UnregisterLatent(modelRelationship.Value, this);
        }
        modelId = null;
        modelRelationship = null;
    }
}
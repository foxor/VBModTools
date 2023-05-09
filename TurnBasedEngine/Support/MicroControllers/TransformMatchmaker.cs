using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class TransformMatchmaker {
    public static Dictionary<ulong, List<Transform>> _registeredParents;
    public static Dictionary<ulong, List<Transform>> RegisteredParents {
        get {
            if (_registeredParents == null) {
                _registeredParents = new Dictionary<ulong, List<Transform>>();
            }
            return _registeredParents;
        }
    }
    public static Dictionary<ulong, List<Transform>> _pendingChildren;
    public static Dictionary<ulong, List<Transform>> PendingChildren {
        get {
            if (_pendingChildren == null) {
                _pendingChildren = new Dictionary<ulong, List<Transform>>();
            }
            return _pendingChildren;
        }
    }
    public static void RegisterParent(ulong o, Transform parent) {
        if (RegisteredParents.TryGetValue(o, out var parents)) {
            if (RegisteredParents[o].Contains(parent)) {
                return;
            }
        }
        else {
            parents = new List<Transform>();
            RegisteredParents[o] = parents;
        }
        parents.Add(parent);
        if (PendingChildren.ContainsKey(o)) {
            foreach (var pendingClient in PendingChildren[o]) {
                pendingClient.SetParent(parent, true);
                pendingClient.gameObject.SetActive(true);
            }
            PendingChildren.Remove(o);
        }
    }
    // When transforms look for a parent, the first one is used.  This can force this parent to be first.
    public static void PrioritizeParent(ulong o, Transform parent) {
        if (RegisteredParents.TryGetValue(o, out var parents)) {
            parents.Remove(parent);
            parents.Insert(0, parent);
        }
    }
    // Returns whether a parent was found immediately.
    public static bool FindParent(ulong o, Transform child) {
        if (RegisteredParents.TryGetValue(o, out var parents)) {
            child.SetParent(parents.First(), true);
            return true;
        }
        else {
            child.gameObject.SetActive(false);
            if (PendingChildren.TryGetValue(o, out var pendingList)) {
                pendingList.Add(child);
            }
            else {
                PendingChildren.Add(o, new List<Transform>() { child });
            }
            return false;
        }
    }
    public static void UnregisterParent(ulong o, Transform parent) {
        if (RegisteredParents.TryGetValue(o, out var parents)) {
            parents.Remove(parent);
            if (!parents.Any()) {
                RegisteredParents.Remove(o);
            }
        }
    }
}
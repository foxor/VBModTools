using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum Dependency : byte {
    All = 0xFF,
    None = 0x00,
    World = 0x01,
    Player = 0x02,
    Monsters = 0x04,
}

[TypeIndex(34)]
public class Model : SObj, IDisposable {
    // If IDs leak into persistence, it'll wreck havoc when we try and load them
    [HideInInspector]
    // MVC has special code that prevents an ID set from being propogated.
    // This only has a setter so that the tools can automatically delete any ids that leak
    public SInt Id { get { return Get<SInt>(0); } set { Set(0, value); } }
    public bool HasId { get => HasProperty(0) && !SNull.IsNull(Id); }
    public bool Destroied { get; protected set; }

    protected virtual void ModelInitialize(){}

    public void Setup() {
        if (HasId) {
            ModelViewController.Instance.VerifyModelIdCorrect(this);
            return;
        }
        ModelViewController.Instance.OnModelCreated(this);
        ModelInitialize();
    }

    public void CreateView<V, M>(Action<V> callback = null) where V : View<M> where M : Model {
        ModelViewController.Instance.CreateView((M)this, callback);
    }
    public void CreateLatentView<V, M>() where V : View<M> where M : Model, ILatent {
        ModelViewController.Instance.CreateView<LatentView<V, M>, M>((M)this);
    }
    public virtual void Dispose() {
        if (TestDetector.AreTestsActive) {
            return;
        }
        Destroied = true;
        ModelViewController.Instance.PreSerializableUnlinked(null, this);
        if (HasId) {
            Remove(0);
        }
    }
    public virtual Dependency GetDependencies() => Dependency.All;
}
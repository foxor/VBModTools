using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModelInstantLock : ILock {
    public bool Write { get; protected set; }
    public byte Dependency { get; set; }
    public float EstimatedDuration { get { return 0f; } }
    public float StartTime { get; set; }
    public ModelInstantLock(Dependency Dependency, bool Write = true) {
        this.Dependency = (byte)Dependency;
        this.Write = Write;
    }
    public abstract string DebugString();

    protected abstract void OnOpen();
    public void Enter(Action<ILock> OnExit) {
        OnOpen();
        if (OnExit != null) {
            OnExit(this);
        }
    }
}
public class ModelCallbackLock : ModelInstantLock {
    protected Action Callback;
    protected string Description;
    public ModelCallbackLock(Dependency Dependency, string Description, Action Callback, bool Write) : base(Dependency, Write) {
        this.Callback = Callback;
        this.Description = Description;
    }
    public override string DebugString() {
        return Description;
    }

    protected override void OnOpen() {
        Callback();
    }
}
public class ModelCallbackReadLock : ModelCallbackLock {
    public ModelCallbackReadLock(Dependency Dependency, string Description, Action Callback) : base(Dependency, Description, Callback, false) {}
}
public class ModelCallbackWriteLock : ModelCallbackLock {
    public ModelCallbackWriteLock(Dependency Dependency, string Description, Action Callback) : base(Dependency, Description, Callback, false) {}
}
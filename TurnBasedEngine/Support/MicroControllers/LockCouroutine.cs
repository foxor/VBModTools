using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCouroutine : ILock {
    public byte Dependency { get; protected set; }
    public bool Write { get; set; }
    public float EstimatedDuration { get; protected set; }
    public float StartTime { get; set; }
    protected IEnumerator Coroutine;
    protected Func<IEnumerator> CoroutineGenerator;
    protected Action<ILock> OnComplete;
    protected string Description;
    protected Action Callback;
    public LockCouroutine(IEnumerator Coroutine, Dependency Dependency, float EstimatedDuration, string Description, Action Callback = null) {
        this.Coroutine = Coroutine;
        this.Dependency = (byte)Dependency;
        this.Write = true;
        this.EstimatedDuration = EstimatedDuration;
        this.Description = Description;
        this.Callback = Callback;
    }
    public LockCouroutine(Func<IEnumerator> CoroutineGenerator, Dependency Dependency, float EstimatedDuration, string Description, Action Callback = null) {
        this.CoroutineGenerator = CoroutineGenerator;
        this.Dependency = (byte)Dependency;
        this.Write = true;
        this.EstimatedDuration = EstimatedDuration;
        this.Description = Description;
        this.Callback = Callback;
    }

    public void Enter(Action<ILock> OnComplete) {
        Assert.That(this.OnComplete == null, $"Attempted to re-open a previously opened lock");
        if (Callback != null) {
            this.OnComplete = (x) => {
                OnComplete(x);
                Callback();
            };
        }
        else {
            this.OnComplete = OnComplete;
        }
        if (Coroutine == null) {
            Coroutine = CoroutineGenerator();
        }
        CoroutineRunner.Run(Coroutine, OnCouroutineComplete);
    }

    protected void OnCouroutineComplete() {
        OnComplete(this);
    }

    public string DebugString() {
        return Description;
    }
}
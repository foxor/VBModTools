using System;
using System.Collections;
using UnityEngine;
public class CoroutineLock : ILock {
    public bool Write { get; protected set; }
    public byte Dependency { get; set; }
    public float EstimatedDuration { get { return float.PositiveInfinity; } }
    public float StartTime { get; set; }
    protected IEnumerator coroutine;
    public CoroutineLock(Dependency Dependency, IEnumerator coroutine) {
        this.Dependency = (byte)Dependency;
        this.coroutine = coroutine;
        Write = true;
    }
    public string DebugString() => "Waiting for coroutine";
    protected IEnumerator AwaitClickCoroutine(Action<ILock> OnExit) {
        // Collapse this layer, so that we don't introduce a frame
        while (coroutine?.MoveNext() == true) {
            yield return coroutine.Current;
        }
        OnExit(this);
    }
    public void Enter(Action<ILock> OnExit) {
        CoroutineRunner.Run(AwaitClickCoroutine(OnExit));
    }
}
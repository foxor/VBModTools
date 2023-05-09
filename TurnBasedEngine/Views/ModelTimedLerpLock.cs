using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelTimerLock : ILock {
    public bool Write { get; set; } = false;
    public byte Dependency { get; set; }
    public float StartTime { get; set; }
    public float EstimatedDuration { get; protected set; }
    protected string Description;
    public ModelTimerLock(Model model, float duration, string description) {
        this.Dependency = (byte)model.GetDependencies();
        EstimatedDuration = duration;
        this.Description = description;
    }
    public string DebugString() => Description;
    protected virtual void Update(float progress) {}
    public virtual void Enter(Action<ILock> OnExit) {
        CoroutineRunner.Run(Tween.FloatTween(EstimatedDuration, 0f, 1f, x => {
            Update(x);
            if (x == 1f) {
                Debug.Log("Done!");
                OnExit(this);
            }
        }));
    }
}

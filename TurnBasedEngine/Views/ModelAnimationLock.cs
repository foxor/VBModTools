using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class ModelAnimationLock : MonoBehaviour, ILock {
    public byte Dependency { get; protected set; }
    public float EstimatedDuration { get; protected set; }
    public float StartTime { get; set; }

    public bool Write { get; set; }
    public AnimationClip Clip;
    private bool InProgress = false;
    public void Setup(Model model) {
        Dependency = (byte)model.GetDependencies();
        EstimatedDuration = Clip.length;
    }
    public void Enter(Action<ILock> OnComplete) {
        StartCoroutine(Run(OnComplete));
    }
    protected IEnumerator Run(Action<ILock> OnComplete) {
        Assert.That(!InProgress, $"Can't start {Clip.name}, It's already animating");
        InProgress = true;
        var animation = GetComponent<Animation>();
        animation.clip = Clip;
        animation.Play();
        yield return new WaitForSeconds(Clip.length);
        InProgress = false;
        OnComplete(this);
    }
    public string DebugString() {
        return $"Trying to play {Clip.name} on {gameObject.name}";
    }
}
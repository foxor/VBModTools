using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ILock {
    bool Write { get; }
    byte Dependency { get; }
    float EstimatedDuration { get; }
    float StartTime { get; set; }
    string DebugString();
    void Enter(Action<ILock> OnExit);
}

public class LockController {
    protected static LockController instance;
    public static LockController Instance {
        get {
            if (instance == null) {
                instance = new LockController();
                if (Application.isPlaying) {
                    CoroutineRunner.Run(instance.Supervise());
                }
            }
            return instance;
        }
    }

    protected int[] PendingWriteRefCounts = new int[8];
    protected byte OpenDeps = 0x00;
    protected List<ILock> OpenLocks = new List<ILock>();
    protected List<ILock> WaitingLocks = new List<ILock>();
    protected List<ILock> OpeningLocks = new List<ILock>();
    protected bool Returning;

    // Careful with this one.  The game can generate thousands of log lines per frame.
    protected static readonly bool DEBUGGING = false;

    public IEnumerable<ILock> GetOpenLocks() {
        return OpenLocks;
    }

    public static bool IsBusy => Instance.GetOpenLocks().Any();

    protected void AddPendingWrite(ILock iLock, int[] LocalPendingWriteRefCounts) {
        var dep = iLock.Dependency;
        for (int i = 0; i < 8; i++) {
            if (((dep >> i) & 1) == 1) {
                LocalPendingWriteRefCounts[i]++;
            }
        }
    }
    protected void RemovePendingWrite(ILock iLock) {
        var dep = iLock.Dependency;
        for (int i = 0; i < 8; i++) {
            if (((dep >> i) & 1) == 1) {
                Assert.That(PendingWriteRefCounts[i] > 0, "Can't remove a pending write, there was no pending write");
                PendingWriteRefCounts[i]--;
            }
        }
    }

    protected bool OpenBlocked(ILock iLock, int[] LocalPendingWriteKeys, byte LocalOpenIds) {
        var Blocked = false;
        var dep = iLock.Dependency;
        for (int i = 0; i < 8; i++) {
            if (((dep >> i) & 1) == 1) {
                Blocked |= LocalPendingWriteKeys[i] > 0;
            }
        }
        if (iLock.Write) {
            Blocked |= ((LocalOpenIds & dep) != 0);
        }
        return Blocked;
    }

    protected void RequestInternal(ILock iLock, List<ILock> OpeningLocks = null) {
        if (iLock == null) {
            return;
        }
        if (DEBUGGING) {
            Debug.Log($"Requesting lock: {iLock.DebugString()}\nDependencies: {iLock.Dependency}");
        }

        var Blocked = OpenBlocked(iLock, PendingWriteRefCounts, OpenDeps);

        if (iLock.Write) {
            AddPendingWrite(iLock, PendingWriteRefCounts);
        }

        if (Blocked) {
            if (DEBUGGING) {
                Debug.Log($"Waiting with {WaitingLocks.Count} others");
            }
            WaitingLocks.Add(iLock);
        }
        else {
            OpenLock(iLock, OpeningLocks);
        }
    }
    public void Request(ILock iLock) {
        RequestInternal(iLock);
    }

    protected void OpenLock(ILock iLock, List<ILock> OpeningLocks = null) {
        OpenLocks.Add(iLock);
        OpenDeps |= iLock.Dependency;
        iLock.StartTime = Time.time;
        if (DEBUGGING) {
            Debug.Log($"Opening lock: {iLock.DebugString()}\nDependencies: {iLock.Dependency}");
        }
        if (OpeningLocks == null) {
            iLock.Enter(ReturnLock);
        }
        else {
            OpeningLocks.Add(iLock);
        }
    }

    // There would be some performance value to making this public, instead of creating actions to wrap it,
    // but maybe locks will want to call other functions when complete?
    protected void ReturnLock(ILock iLock) {
        if (DEBUGGING) {
            Debug.Log($"Lock returned: {iLock.DebugString()}\nDependencies: {iLock.Dependency}");
        }
        OpenLocks.Remove(iLock);
        byte newDeps = 0;
        foreach (var ilock in OpenLocks) {
            newDeps |= ilock.Dependency;
        }
        if (OpenDeps != newDeps) {
            OpenDeps = newDeps;
        }
        else {
            // If there is a pending lock, we'll need to wait for them to resolve it.
            // If there isn't one, there's no point in looking through the pending queue
            // This does assume that locks with the same id have the same dependencies.
            return;
        }
        for (int i = 0; i < 8; i++) {
            PendingWriteRefCounts[i] = 0;
        }

        var rootCall = !Returning;
        if (rootCall) {
            Returning = true;
        }

        var previouslyWaitingLocks = WaitingLocks;
        WaitingLocks = Constructor.Construct<List<ILock>>();
        foreach (var potentiallyOpenLock in previouslyWaitingLocks) {
            RequestInternal(potentiallyOpenLock, OpeningLocks);
        }
        previouslyWaitingLocks.Clear();
        Pool.Return(previouslyWaitingLocks);

        if (rootCall) {
            int lockCycles = 0;
            while (OpeningLocks.Any()) {
                Assert.That(lockCycles++ < 10000, "Infinite lock loop?");
                var opening = OpeningLocks;
                OpeningLocks = Constructor.Construct<List<ILock>>();
                foreach (var newlyOpenLock in opening) {
                    // All the callbacks are called once all the open locks are calculated to avoid re-entrancy issues.
                    newlyOpenLock.Enter(ReturnLock);
                }
                opening.Clear();
                Pool.Return(opening);
            }
        }
        if (!OpenLocks.Any() && WaitingLocks.Any()) {
            Debug.LogError("Deadlock!");
        }

        if (rootCall) {
            Returning = false;
        }
    }
    protected IEnumerator Supervise() {
        var wait = new WaitForSeconds(0.5f);
        while (true) {
            yield return wait;
            foreach (var openLock in OpenLocks) {
                // Assume this frame isn't twice as long as the last one
                var activeDuration = Time.time - openLock.StartTime - Time.deltaTime * 2f;
                Assert.That(activeDuration < openLock.EstimatedDuration * 1.2f, $"A lock claimed it needed {openLock.EstimatedDuration}, but has not called back for {activeDuration}s: {openLock.DebugString()}");
            }
            if (!OpenLocks.Any() && WaitingLocks.Any()) {
                Debug.LogError("Deadlock!");
            }
        }
    }
}
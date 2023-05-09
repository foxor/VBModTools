using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AssertionFailure : Exception {
    protected int purgeDepth = 1;
    public AssertionFailure(string message) : base(message) {}
    public AssertionFailure(string message, int purgeDepth) : base(message) {
        this.purgeDepth = purgeDepth;
    }
    // We don't want to look at the stack frame where the assert actually throws,
    // we want to look at the line that calls Assert(false)
    public override string StackTrace => string.Join("\n", base.StackTrace.Split('\n').Skip(purgeDepth));
}

public static class Assert {
    [Conditional("ASSERTS_ENABLED")]
    public static void That(bool invariant, string errorMessage = "") {
        if (!invariant) {
            if (!string.IsNullOrEmpty(errorMessage)) {
                throw new AssertionFailure(errorMessage);
            }
            else {
                throw new AssertionFailure("Assert Failed");
            }
        }
    }
    [Conditional("ASSERTS_ENABLED")]
    public static void That(bool invariant, int stackPurgeDepth, string errorMessage = "") {
        if (!invariant) {
            if (!string.IsNullOrEmpty(errorMessage)) {
                throw new AssertionFailure(errorMessage, stackPurgeDepth);
            }
            else {
                throw new AssertionFailure("Assert Failed", stackPurgeDepth);
            }
        }
    }
}
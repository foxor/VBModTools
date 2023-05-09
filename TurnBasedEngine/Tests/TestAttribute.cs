using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TestAbstractionLevel {
    Debug = 0,
    LowLevel = 1,
    Foundational = 2,
    HighLevel = 3,
    Performance = 4,
    Content = 5,
}

[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : System.Attribute {
    public TestAbstractionLevel Level;
    public TestAttribute(TestAbstractionLevel Level = TestAbstractionLevel.HighLevel) {
        this.Level = Level;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TestMustThrowAttribute : System.Attribute {
    public TestMustThrowAttribute() {
    }
}
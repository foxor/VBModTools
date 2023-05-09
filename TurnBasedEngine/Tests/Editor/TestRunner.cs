using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

[ProtectStatics]
[InitializeOnLoad]
public class TestRunner {
    // I encourage you to disable asserts if you want to run speed tests
    public static readonly bool ENABLE_SPEED_TESTS = false;
    protected class TestMethodInfo {
        public MethodInfo Method;
        public TestAbstractionLevel Level;
        public bool MustThrow = false;
    }

    protected static MethodInfo FirstFailedTest;
    protected static MethodInfo LastFailedTest;

    static TestRunner() {
        try {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.ClearDeveloperConsole();
                RunTests();
            }
        }
        catch (Exception e) {
            Debug.LogError("Uncaught exception thrown during test: " + e.ToString());
        }
    }

    protected static IEnumerable<TestMethodInfo> AllTests() {
        foreach (Type t in StaticResetTool.DomainTypes) {
            foreach (MethodInfo method in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
                if (!method.IsStatic || method.GetParameters().Length != 0) {
                    continue;
                }
                TestMethodInfo testMethod = null;
                foreach (CustomAttributeData attribute in method.CustomAttributes) {
                    if (attribute.AttributeType == typeof(TestAttribute)) {
                        if (testMethod == null) {
                            testMethod = new TestMethodInfo();
                        }
                        testMethod.Method = method;
                        if (attribute.ConstructorArguments.Count > 0) {
                            testMethod.Level = (TestAbstractionLevel)attribute.ConstructorArguments[0].Value;
                        }
                        else {
                            testMethod.Level = TestAbstractionLevel.HighLevel;
                        }
                        continue;
                    }
                    if (attribute.AttributeType == typeof(TestMustThrowAttribute)) {
                        if (testMethod == null) {
                            testMethod = new TestMethodInfo();
                        }
                        testMethod.MustThrow = true;
                    }
                }
                if (testMethod != null && testMethod.Method != null) {
                    yield return testMethod;
                }
            }
        }
    }

    private class TestFailure {
        public string testName;
        public Exception exception;
    }

    [MenuItem("Tools/Run Tests")]
    public static void RunTests() {
        RunTests(false);
    }

    [MenuItem("Tools/Run Content Tests")]
    public static void RunContentTests() {
        RunTests(true);
    }

    protected static void RunTests(bool content) {
        StaticResetTool.Reset();
        return;
        // I care about the iteration time, so initialization counts
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var passed = new List<string>();
        var failed = new List<TestFailure>();
        var unattempted = new List<string>();
        var testSet = AllTests();
        foreach (TestAbstractionLevel level in (TestAbstractionLevel[])Enum.GetValues(typeof(TestAbstractionLevel))) {
            if (failed.Any()) {
                unattempted.Add(level.ToString());
                continue;
            }
            if (level == TestAbstractionLevel.Performance && !ENABLE_SPEED_TESTS) {
                continue;
            }
            if ((level == TestAbstractionLevel.Content) != content) {
                continue;
            }
            foreach (var testMethod in testSet.Where(x => x.Level == level)) {
                string testName = testMethod.Method.DeclaringType + "::" + testMethod.Method.Name;
                bool testPassed = false;
                System.Exception exception = null;
                TestDetector.AreTestsActive = true;
                try {
                    testMethod.Method.Invoke(null, null);
                    testPassed = !testMethod.MustThrow;
                }
                catch (System.Reflection.TargetInvocationException e) {
                    testPassed = testMethod.MustThrow;
                    exception = e;
                }
                finally {
                    StaticResetTool.Reset();
                }
                if (testPassed) {
                    passed.Add(testName);
                }
                else {
                    failed.Add(new TestFailure() { testName = testName, exception = exception });
                    LastFailedTest = testMethod.Method;
                    if (FirstFailedTest == null) {
                        FirstFailedTest = testMethod.Method;
                    }
                }
            }
        }
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        if (failed.Count == 0) {
            Debug.Log($"All tests passed ({elapsedMs} ms, {passed.Count} / {passed.Count})");
        }
        else {
            foreach (var failedTest in failed) {
                if (failedTest.exception == null) {
                    Debug.LogError($"Failed {failedTest.testName}.  No exception was thrown when one was expected.");
                }
                else {
                    Debug.LogError("Failed " + failedTest.testName);
                    Debug.LogException(failedTest.exception);
                }
            }
            Debug.Log("Passed tests (" + passed.Count + "/" + (passed.Count + failed.Count) + "):\n\t" + string.Join("\n\t", passed));
            Debug.Log("Failed tests (" + failed.Count + "/" + (passed.Count + failed.Count) + "):\n\t" + string.Join("\n\t", failed.Select(x => x.testName)));
            if (unattempted.Any()) {
                Debug.Log("Didn't attempt tests at levels: " + string.Join(", ", unattempted));
            }
        }
    }

    [MenuItem("Tools/Run First Failed Test")]
    public static void RunFirstFailedTest() {
        if (FirstFailedTest == null) {
            Debug.Log("No tests failed!  XD");
        }
        else {
            Debug.Log($"Re-attempting {FirstFailedTest.Name}:");
            TestDetector.AreTestsActive = true;
            try {
                FirstFailedTest.Invoke(null, null);
            }
            finally {
                StaticResetTool.Reset();
            }
        }
    }
}
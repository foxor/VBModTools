using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTests {

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestEventFires() {
        MediatedTypedEvent<object> TestEvent = new MediatedTypedEvent<object>();
        bool fired = false;
        Action<object> method = (_) => {fired = true;};
        TestEvent.Register(method);
        TestEvent.Invoke(null);
        if (!fired) {
            throw new Exception("Closure captured bool indicates registration failed!");
        }
    }

    [Test(TestAbstractionLevel.Foundational)]
    protected static void TestObjectEquality() {
        MediatedTypedEvent<object> TestEvent = new MediatedTypedEvent<object>();
        bool fired = false;
        Action<object> method = (_) => {fired = true;};
        object a = new SInt();
        object b = new SInt();
        TestEvent.Register(method, a);
        TestEvent.Invoke(b);
        if (fired) {
            throw new Exception("Event called even though event was triggered for the wrong source object");
        }
    }

    [Test(TestAbstractionLevel.Foundational)]
    protected static void TestAlwaysPredicate() {
        MediatedTypedEvent<object> TestEvent = new MediatedTypedEvent<object>();
        bool fired = false;
        Action<object> method = (_) => {fired = true;};
        TestEvent.Register(new MediatedTypedEvent<object>.Registration(){ Qualifier = (_) => { return true; }, Fire = method});
        TestEvent.Invoke();
        if (!fired) {
            throw new Exception("Event didn't fire even though qualifier is always true");
        }
    }
}

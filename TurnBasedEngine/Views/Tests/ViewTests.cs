using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = System.Object;

public static class ViewTests {
    [TypeIndex(37)]
    public class TestModel : Model {
        public Model Top {get; set;}
        public Model Bottom {get; set;}
    }
    [Test(TestAbstractionLevel.Foundational)]
    public static void TestViewInit() {
        TestDetector.AreTestsActive = false; // This test makes views which is normally disabled
        using (var t = new TestModel()) {
            try {
                t.Setup();
            }
            catch (Exception e) {
                Debug.LogError(e);
                throw e;
            }
        }
    }

    public class IdenticalObject {
        public override bool Equals(object obj) => true;
        public override int GetHashCode() => 0;
        public static bool operator==(IdenticalObject a, IdenticalObject b) => true;
        public static bool operator!=(IdenticalObject a, IdenticalObject b) => false;
    }
    [Test(TestAbstractionLevel.LowLevel)]
    public static void IdenticalObjectsHashable() {
        IdenticalObject a = new IdenticalObject();
        IdenticalObject b = new IdenticalObject();
        Dictionary<object, int> map = new Dictionary<object, int>();
        map[a] = 3;
        if (map[a] != 3) {
            throw new Exception("Couldn't put entry in map");
        }
        map[b] = 2;
        if (map[a] != 2) {
            throw new Exception("Second entry didn't overwrite first");
        }
    }

    public class AlmostIdenticalObject {
        public override bool Equals(object obj) => Object.ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
        public static bool operator==(AlmostIdenticalObject a, AlmostIdenticalObject b) => true;
        public static bool operator!=(AlmostIdenticalObject a, AlmostIdenticalObject b) => false;
    }
    [Test(TestAbstractionLevel.LowLevel)]
    public static void AlmostIdenticalObjectsHashable() {
        AlmostIdenticalObject a = new AlmostIdenticalObject();
        Dictionary<object, int> map = new Dictionary<object, int>();
        map[a] = 3;
        if (map[a] != 3) {
            throw new Exception("Couldn't put entry in map");
        }
        // You can make this a bigger number, but it works 100,000,000 causes a collision if Equals => true
        for (int i = 0; i < 100; i++) {
            AlmostIdenticalObject b = new AlmostIdenticalObject();
            map[b] = 2;
        }
        if (map[a] != 3) {
            throw new Exception("Some entry overwrote first");
        }
    }
    public static void DependenciesEqual(Model model, byte IntendedDependencies) {
        var dependencies = (byte)model.GetDependencies();
        Assert.That(dependencies == IntendedDependencies,
            $"Wrong dependencies.  Have [{ dependencies}] wanted [{IntendedDependencies}]");
    }

    [Test(TestAbstractionLevel.LowLevel)]
    public static void TestModelLinks() {
        TestDetector.AreTestsActive = false; // This test makes views which is normally disabled
        var a = new TestModel();
        a.Setup();
        var b = new SObj();
        var c = new TestModel();
        c.Setup();
        var d = new SObj();
        a.Set(1, b);
        b.Set(0, c);
        c.Set(1, d);
        ModelViewController.ModelRelationShip relationship;
        ModelViewController.Instance.GetModelRelationship(out relationship, a);
        Assert.That(relationship.ModelId == a.Id);
        ModelViewController.Instance.GetModelRelationship(out relationship, b);
        Assert.That(relationship.ModelId == a.Id);
        ModelViewController.Instance.GetModelRelationship(out relationship, c);
        Assert.That(relationship.ModelId == c.Id);
        ModelViewController.Instance.GetModelRelationship(out relationship, d);
        Assert.That(relationship.ModelId == c.Id);
    }
}
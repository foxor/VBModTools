using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using UnityEngine;

public class SerializationTests {

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestBoolSerialize() {
        SBool b = true;
        var stream = b.ToStream();
        b = stream.Consume<SBool>();
        if (!b) {
            throw new System.Exception("test bool came back false!");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestPropertySerialize() {
        SObj s = new SObj();
        s.Set(0, new SInt(){Value = 123});
        var stream = s.ToStream();
        s = stream.Consume<SObj>();
        var newVal = s.Get<SInt>(0).Value;
        if (newVal != 123) {
            throw new System.Exception($"Number property changed!  Now it's {newVal} instead of 123");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestNestedGenericSerialize() {
        var s = Constructor.Construct<SList<SList<SList<SInt>>>>();
        s.Add(Constructor.Construct<SList<SList<SInt>>>());
        s[0].Add(Constructor.Construct<SList<SInt>>());
        s[0][0].Add(new SInt(64));
        var stream = s.ToStream();
        s = stream.Consume<SList<SList<SList<SInt>>>>();
        if (s[0][0][0].GetType() != typeof(SInt)) {
            throw new System.Exception("generic types not preserved!");
        }
        if (s[0][0][0].Value != 64) {
            throw new System.Exception("value not preserved");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestSerializeString() {
        var t = new SString() { Value = "Test" };
        var stream = t.ToStream();
        var b = stream.Consume<SString>();
        if (b != "Test") {
            throw new System.Exception("string didn't survive!");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestSerializeList() {
        var l = new SList<SString>() { innerList = new List<SString>(){ "a", "b", "c"} };
        var stream = l.ToStream();
        var b = stream.Consume<SList<SString>>();
        if (b.innerList[2].Value != "c") {
            throw new System.Exception("List didn't survive!");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestPolymorphicList() {
        var l = new SList<ISerializable>();
        l.Add(new SBool(true));
        l.Add(new SInt(5));
        var stream = l.ToStream();
        var b = stream.Consume<SList<ISerializable>>();
        if (!(b.innerList[0] as SBool).Value) {
            throw new System.Exception("bool didn't survive!");
        }
        if ((b.innerList[1] as SInt).Value != 5) {
            throw new System.Exception("number didn't survive!");
        }
    }

    [SerializableEnum]
    [TypeIndex(35)]
    protected enum TestEnum : byte {
        OptionA,
        OptionB,
        OptionC
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void CanSerializeEnums() {
        if (new SEnum<TestEnum>(TestEnum.OptionB).ToStream().Consume<SEnum<TestEnum>>() != TestEnum.OptionB) {
            throw new System.Exception("Enum value not preserved during serialization");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void NoInsaneTypes() {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            if (!assembly.FullName.Contains("CSharp")) {
                continue;
            }
            foreach (Type t in assembly.GetTypes()) {
                if (t.GenericTypeArguments.Length > 255) {
                    throw new Exception("Some crazy type has too many args");
                }
            }
        }
    }

    [Test(TestAbstractionLevel.Performance)]
    protected static void SpeedTestSobjSerialization() {
        var branchingFactors = new int[]{10, 6, 4, 2, 1, 1};
        var leafFactories = new Func<ISerializable>[] {
            () => { return new SInt(); },
            () => { return new SString(); },
            () => { return new SBool(); },
            () => { return new SInt2(); },
            () => { return new SEnum<TestEnum>(TestEnum.OptionA); },
            () => { return new SNull(); },
        };
        var leafCount = 0;
        Func<ISerializable> MakeNextLeaf = () => {
            var factory = leafFactories[(leafCount++) % leafFactories.Length];
            return factory();
        };
        var i = 0;
        Func<int> IndexFactory = () => {
            return i++;
        };
        Func<int, ISerializable> ProduceObj = null;
        ProduceObj = (int level) => {
            if (level >= branchingFactors.Length - 1) {
                return MakeNextLeaf();
            }
            var obj = new SObj();
            for (int childIndex = 0; childIndex < branchingFactors[level]; childIndex++) {
                obj.Set(IndexFactory(), ProduceObj(level + 1));
            }
            return obj;
        };

        // Make giant object.  This is slow, so don't time it.
        var world = ProduceObj(0);
        
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();  

        var worldCopy = (world as SObj).DeepCopy<SObj>();

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        if (ts.TotalMilliseconds > 15) {
            // it took 20.382 on the first try
            // 7.298 after 2 rounds of optimization (with asserts off)
            throw new Exception($"It took {ts.TotalMilliseconds} to copy the world.  Expected 10 or less");
        }
    }

    [Test(TestAbstractionLevel.HighLevel)]
    public static void TestEnumListToString() {
        var l = new SList<SEnum<TestEnum>>();
        l.Add((SEnum<TestEnum>)TestEnum.OptionA);
        l.Add((SEnum<TestEnum>)TestEnum.OptionB);
        var serialized = l.ToString();
        if (!serialized.Equals("[OptionA, OptionB]")) {
            throw new Exception($"List didn't serialize right.  Wanted: \"[OptionA, OptionB]\", got: \"{serialized}\"");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    public static void TestCanSerializeNullStringInList() {
        var l = new SList<SString>();
        l.Add(null);
        var c = l.DeepCopy();
        Assert.That(c[0] == null, "Null value didn't survive serialization");
    }

    [Test(TestAbstractionLevel.LowLevel)]
    public static void TestAllEnumsHaveAttribute() {
        foreach (var t in Stream.AssemblyTypes.Where(x => x.IsEnum)) {
            Assert.That(t.GetCustomAttribute<SerializableEnumAttribute>() != null, $"{t.GetNameCached()} doesn't have a SerializableEnum attribute");
        }
    }

    [Test(TestAbstractionLevel.LowLevel)]
    public static void TestSerializableIndicies() {
        var shouldHaveIndex = Assembly.GetExecutingAssembly().GetTypes().Where(x => Stream.IsAssemblyType(x));
        var usedIndicies = new List<string>();
        var typesMissingAttribute = new List<Type>();
        foreach (var t in shouldHaveIndex) {
            if (!t.HasIndex()) {
                typesMissingAttribute.Add(t);
                continue;
            }
            var index = t.GetIndex();
            Assert.That(usedIndicies.Count <= index || usedIndicies[index] == null, $"{t.GetNameCached()} uses an already used index");
            while (usedIndicies.Count <= index) {
                usedIndicies.Add(null);
            }
            usedIndicies[index] = t.GetNameCached();
        }
        for (int index = 0; index < usedIndicies.Count; index++) {
            if (usedIndicies[index] == null) {
                // This used to be an error, but it shouldn't be, because things can't always be moved to fill the gap
                UnityEngine.Debug.LogWarning($"Type index {index} is empty!  Maybe put {usedIndicies[usedIndicies.Count - 1]} in there?  It's last.");
                break;
            }
        }
        if (typesMissingAttribute.Any()) {
            var nextIndex = usedIndicies.Count();
            var log = "Some types are constructable and serializable, but have no index:\n";
            foreach (var t in typesMissingAttribute) {
                log += $"{nextIndex++} - {t.GetNameCached()}\n";
            }
            Assert.That(false, log);
        }
    }
}
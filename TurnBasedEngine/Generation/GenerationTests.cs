using System.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GenerationTests {
    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGeneratePrimative() {
        Generator.Generate<SBool>();
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateInterface() {
        Generator.Generate<ICastable<bool>>();
    }

    public interface IGenericTest { }
    [TypeIndex(11)]
    public class GenerationTestGeneric<T> : IGenericTest, IGenerateable where T : ICastable<bool> { }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateGeneric() {
        Assert.That(typeof(IGenericTest).IsAssignableFrom(typeof(GenerationTestGeneric<SBool>)));
        Generator.Generate<IGenericTest>();
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateConcrete() {
        Generator.Generate<GenerationTestGeneric<ICastable<bool>>>();
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestEliminateInvalidGenericArguments() {
        var GeneratedBool = Generator.Generate<IGenericTest>();
        if (!(GeneratedBool.GetType().GetGenericTypeDefinition() == typeof(GenerationTestGeneric<>))) {
            throw new Exception("Failed to collapse interface!");
        }
    }

    public interface IGenericTest2 { }
    [TypeIndex(12)]
    public class TestNullObject : SNull, IGenericTest2 {}

    [Test(TestAbstractionLevel.LowLevel)]
    public static void TestCanGenerateNullBacked() {
        Assert.That(Generator.CreateConstructableTypes(typeof(IGenericTest2)).Any(x => x == typeof(TestNullObject)));
    }
    
    public interface IDoubleGenericTest { }
    [TypeIndex(13)]
    public class GenerationTestDoubleGeneric<T, V> : IDoubleGenericTest, IGenerateable where T : GenerationTestGeneric<V> where V : ICastable<bool> { }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateDoubleGeneric() {
        var types = Generator.GetConstructableTypes(typeof(IDoubleGenericTest));
        // One where V is ICastable<bool> and one where it's SBool
        Assert.That(types.Count() == 2);
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestPreserveInterfaceArguments() {
        var types = Generator.GetConstructableTypes(typeof(SList<ISerializable>));
        Assert.That(types.Any(x => x.GenericTypeArguments[0] == typeof(ISerializable)));
    }

    [TypeIndex(36)]
    public class TestFillInClass : IAssemblyType {
        [GenerateObject]
        public SBool value;
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestFillIn() {
        var filledIn = (TestFillInClass)Generator.Generate<TestFillInClass>();
        if (filledIn.value == null) {
            throw new Exception("Value flagged with [Generate] didn't get filled in!");
        }
    }

    [TypeIndex(14)]
    public class TestFillInListClass : IAssemblyType {
        [GenerateList]
        public List<SBool> value;
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestFillInList() {
        var filledIn = (TestFillInListClass)Generator.Generate<TestFillInListClass>();
        if (filledIn.value == null) {
            throw new Exception("Value flagged with [Generate] didn't get filled in!");
        }
    }

    [TypeIndex(15)]
    public class TestFillInIEnumerableClass : IAssemblyType {
        [GenerateList]
        public IEnumerable<SBool> value;
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestFillInIEnumerable() {
        var filledIn = (TestFillInIEnumerableClass)Generator.Generate<TestFillInIEnumerableClass>();
        if (filledIn.value == null) {
            throw new Exception("Value flagged with [Generate] didn't get filled in!");
        }
        if (filledIn.value.GetType() != typeof(List<SBool>)) {
            throw new Exception("Value didn't get filled in with a list!");
        }
    }


    [TypeIndex(16)]
    public class TestFillInIEnumerableClassWithLength : IAssemblyType {
        [GenerateList(3)]
        public IEnumerable<SBool> value;
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestFillInIEnumerableWithLength() {
        var filledIn = (TestFillInIEnumerableClassWithLength)Generator.Generate<TestFillInIEnumerableClassWithLength>();
        if (filledIn.value == null) {
            throw new Exception("Value flagged with [Generate] didn't get filled in!");
        }
        if (filledIn.value.GetType() != typeof(List<SBool>)) {
            throw new Exception("Value didn't get filled in with a list!");
        }
        if (filledIn.value.Count() != 3) {
            throw new Exception("Value didn't get filled in with the right length!");
        }
        if (filledIn.value.First() == null) {
            throw new Exception("Value didn't get filled in with non-null values");
        }
    }

    [TypeIndex(17)]
    public class GenerationOrderTest : IAssemblyType {
        [GenerationOrder(1)]
        public int a;
        [GenerationOrder(3)]
        public int c;
        [GenerationOrder(2)]
        public int b;
        public int z;
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerationOrder() {
        var OrderedMemberNames = typeof(GenerationOrderTest).GetMembers().OrderBy(GenerationOrder.GetOrder).Select(ReflectionExtensions.GetNameCached).ToArray();
        int zIndex = OrderedMemberNames.IndexOf(x => x == "z");
        if (OrderedMemberNames[0] != "a" || OrderedMemberNames[1] != "b" || OrderedMemberNames[2] != "c" || zIndex < 2) {
            throw new Exception($"Members are in the wrong order: {String.Join(", ", OrderedMemberNames)}, zIndex: {zIndex}");
        }
    }

    [TypeIndex(18)]
    public class GenerationInjectTest : IAssemblyType {
        [GenerationOrder(1)]
        [GenerateObject]
        public SBool a;
        [GenerationOrder(2)]
        [InjectOnly]
        public SBool b;
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerationInjection() {
        var generated = Generator.GenerateTyped<GenerationInjectTest>();
        var equalBefore = generated.a == generated.b;
        generated.a.Value ^= true;
        var equalAfter = generated.a == generated.b;
        if (!equalBefore || !equalAfter) {
            throw new Exception("Inject fails to establish referential link");
        }
    }

    protected interface IAncestorA {}
    protected interface IAncestorB { }
    protected class ParameterTarget : IGenerateable, IAncestorA, IAncestorB { }
    protected class ParameterConstrainer<T> : IGenerateable where T : IAncestorA, IAncestorB {}

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateDescendedParameterTypes() {
        var types = Generator.GetConstructableTypes(typeof(ParameterConstrainer<>));
        Assert.That(types.Single().GetGenericArguments().Single() == typeof(ParameterTarget));
    }

    protected interface IGrandAncestorC {}
    protected interface IAncestorC : IGrandAncestorC {}
    protected interface IGrandAncestorD {}
    protected interface IAncestorD : IGrandAncestorD { }
    protected class TestGrandChild : IGenerateable, IAncestorC, IAncestorD { }
    protected class TestGrandChildFilter<T> : IGenerateable where T : IGrandAncestorC, IGrandAncestorD {}
    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateParameterGrandchild() {
        var types = Generator.GetConstructableTypes(typeof(TestGrandChildFilter<>));
        Assert.That(types.Single().GetGenericArguments().Single() == typeof(TestGrandChild));
    }

    /*[SerializableEnum]
    protected enum TestEnum : byte {
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenerateEnum() {
        Generator.Generate<Enum>();
        // This doesn't work, but the game probably doesn't need it, so I'll leave it broken.
    }*/

    [GenerateList(3)]
    [TypeIndex(27)]
    protected class TestGenerateListClassAttribute : SList<SBool> {
    }
    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestCanGenerateListClassAttribute() {
        Generator.Generate<TestGenerateListClassAttribute>();
    }

    [TypeIndex(20)]
    protected interface ITestEquivalent : ISerializable { }
    [TypeIndex(28)]
    protected class ITestEquivalentA : SNull, ITestEquivalent { }
    [TypeIndex(29)]
    protected class ITestEquivalentB : SNull, ITestEquivalent { }
    [TypeIndex(30)]
    protected class ITestEquivalentC : SNull, ITestEquivalent {}

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestConstructableTypeNotSubstituted() {
        Generator.GenerateTyped<SList<ITestEquivalent>>();
    }


    [TypeIndex(19)]
    protected interface ITestGeneric<T> : ISerializable { }
    [TypeIndex(31)]
    protected class TestGenericParticularizable<T> : SNull, ITestGeneric<T> {}
    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestGenericParticularization() {
        Generator.Generate<ITestGeneric<SObj>>();
    }

    [TypeIndex(13)]
    protected interface ITestObeyTypeConstraints<T> : ISerializable { }
    [TypeIndex(32)]
    protected class TestObeyTypeConstraintsCorrect : SNull, ITestObeyTypeConstraints<SBool> { }
    [TypeIndex(33)]
    protected class TestObeyTypeConstraintsIncorrect : SNull, ITestObeyTypeConstraints<SObj> {}

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestObeyTypeConstraints() {
        var constrainedTypes = Generator.GetConstructableTypes(typeof(ITestObeyTypeConstraints<SBool>));
        Assert.That(constrainedTypes.Count() == 1);
    }

    [Test(TestAbstractionLevel.LowLevel)]
    protected static void TestCombinationGenerator() {
        var testSequence = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 }, new int[] { 7, 8, 9 } };
        var combinations = testSequence.Permutations();
        Assert.That(combinations.Count() == 27);
        Assert.That(combinations.Where(x => x.First() == 1).Count() == 9);
        Assert.That(combinations.Where(x => x.First() == 2).Count() == 9);
        Assert.That(combinations.Where(x => x.First() == 3).Count() == 9);
        Assert.That(combinations.Where(x => x.Skip(1).First() == 4).Count() == 9);
        Assert.That(combinations.Where(x => x.Skip(1).First() == 5).Count() == 9);
        Assert.That(combinations.Where(x => x.Skip(1).First() == 6).Count() == 9);
        Assert.That(combinations.Where(x => x.Last() == 7).Count() == 9);
        Assert.That(combinations.Where(x => x.Last() == 8).Count() == 9);
        Assert.That(combinations.Where(x => x.Last() == 9).Count() == 9);
        // I'm fairly certain this test is complete?  Is there a proof for this?
        // If any elements were omitted or duplicated, at least 1 of these tests would fail
    }
}

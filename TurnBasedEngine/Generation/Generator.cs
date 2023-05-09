using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public interface IGenerateable {
}

public static class Generator {
    public static List<Type> generatableTypes;
    public static List<Type> GeneratableTypes {
        get {
            if (generatableTypes == null) {
                InitializeTypes();
            }
            return generatableTypes;
        }
    }
	public static Dictionary<Type, List<Type>> implementors;
	public static Dictionary<Type, List<Type>> Implementors {
        get {
            if (implementors == null) {
                InitializeTypes();
            }
            return implementors;
        }
    }
    public static Dictionary<Type, List<object>> createdObjects;
    public static Dictionary<Type, List<object>> CreatedObjects {
        get {
            if (createdObjects == null) {
                createdObjects = new Dictionary<Type, List<object>>();
            }
            return createdObjects;
        }
    }

    public static Dictionary<Type, Type[]> createdTypes;
    public static Dictionary<Type, Type[]> CreatedTypes {
        get {
            if (createdTypes == null) {
                createdTypes = new Dictionary<Type, Type[]>();
            }
            return createdTypes;
        }
    }

    public static void InitializeTypes() {
		implementors = new Dictionary<Type, List<Type>>();
        generatableTypes = new List<Type>();

        Action<Type, Type> AddImplementor = (Type implementor, Type implemented) => {
            if (implemented.IsGenericType) {
                implemented = implemented.GetGenericTypeDefinition();
            }
            if (!implementors.ContainsKey(implemented)) {
                implementors[implemented] = new List<Type>();
            }
            implementors[implemented].Add(implementor);
        };
        
		foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
			if (typeof(IGenerateable).IsAssignableFrom(t)) {
                generatableTypes.Add(t);
                if (t.BaseType != null) {
                    AddImplementor(t, t.BaseType);
                }
                foreach (var Interface in t.GetInterfaces()) {
                    AddImplementor(t, Interface);
                }
			}
		}
	}
    public static Type ChooseWeighted(IEnumerable<Type> PotentialTypes) {
        // TODO: randomize which implementor we choose based on a class decorator with a weight
        return RNG.Generation.Choose(PotentialTypes);
    }
    public static IEnumerable<Type> GetImplementors(Type TypeToImplement) {
        if (TypeToImplement.IsGenericType) {
            TypeToImplement = TypeToImplement.GetGenericTypeDefinition();
        }
        if (Implementors.ContainsKey(TypeToImplement)) {
            return Implementors[TypeToImplement];
        }
        return new Type[0];
    }
    public static IEnumerable<Type> MakeTypes(Type genericType, IEnumerable<IEnumerable<Type>> allTypeArguments) {
        var typeArgumentCombinations = allTypeArguments.Permutations();
        foreach (var typeArgumentCombination in typeArgumentCombinations) {
            Type createdType = null;
            try {
                createdType = genericType.MakeGenericType(typeArgumentCombination.ToArray());
            }
            catch {
                // There are some complex things happening here that make this both unavoidable and fine.
                // It's unavoidable because each type parameter can be valid on its own, but they are invalid together.
                // For example, when a type valiable is used twice.
                // It's fine because this process is already mega slow, so it's memoized at a higher level.
                continue;
            }
            yield return createdType;
        }
    }
    public static bool NoFilter(Type t) {
        return true;
    }
    public static bool ReferenceFilter(Type t) {
        return !t.IsValueType && !t.IsInterface;
    }
    public static Func<Type, bool> GetSpecialTypeConstraintFilter(Type genericParameter) {
        // Roughly based on https://docs.microsoft.com/en-us/dotnet/api/system.type.getgenericparameterconstraints?view=netcore-3.1
        var constraints = genericParameter.GenericParameterAttributes & 
            GenericParameterAttributes.SpecialConstraintMask;
        if (constraints == GenericParameterAttributes.None) {
            return NoFilter;
        }
        else if (constraints == GenericParameterAttributes.ReferenceTypeConstraint) {
            return ReferenceFilter;
        }
        throw new Exception("Unsupported parameter constraint!" + constraints);
    }
    public static IEnumerable<Type> GetParameterTypes(Type t) {
        if (t == typeof(System.Enum)) {
            // While technically valid from a generation perspective, SEnum<Enum> is not currently supported.
            return GetImplementors(t);
        }
        if (t.IsAbstract || t.IsInterface) {
            return new Type[] { t }.Concat(GetImplementors(t));
        }
        if (t.IsGenericParameter) {
            var constraintFilter = GetSpecialTypeConstraintFilter(t);
            var constraints = t.GetGenericParameterConstraints();
            if (constraints.Any()) {
                return constraints.Select(GetParameterTypes).Intersect().Where(constraintFilter);
            }
            else {
                return Stream.AssemblyTypes.Where(constraintFilter);
            }
        }
        return GetConstructableTypes(t);
    }
    public static IEnumerable<Type> GetConstructableTypes(Type t) {
        Func<Type, bool> IsAssignable = (NewType) => {
            if (t.ContainsGenericParameters && t.GetGenericArguments().Any(x => x.IsGenericParameter)) {
                // This is an unclosed generic type, which can't use IsAssignableFrom
                // typeof(List<>).IsAssignableFrom(typeof(List<int>)) is false, for example
                // This isn't technically true, but the GetImplementors filter should remove everything where it's not true.
                // A technically correct solution to this problem is significantly non-trivial
                return true;
            }
            else {
                return t.IsAssignableFrom(NewType);
            }
        };
        if (Constructor.IsRecursivlyConstructableType(t)) {
            var subclasses = GetImplementors(t).Select(GetConstructableTypes).Flatten().Where(IsAssignable);
            return new Type[]{ t }.Concat(subclasses);
        }
        if (t.IsAbstract || t.IsInterface) {
            return GetImplementors(t).Select(GetConstructableTypes).Flatten().Where(IsAssignable);
        }
        if (t.IsGenericParameter) {
            throw new Exception("Cannot construct a type parameter");
        }
        if (t.IsGenericType) {
            var genericTypeDefinition = t.IsGenericTypeDefinition ? t : t.GetGenericTypeDefinition();
            var typeArguments = t.GetGenericArguments().Select(GetParameterTypes);
            return MakeTypes(genericTypeDefinition, typeArguments).Where(IsAssignable);
        }
        throw new Exception($"Failed to create type {t}");
    }
    public static IEnumerable<Type> CreateConstructableTypes(Type type) {
        if (CreatedTypes.TryGetValue(type, out var created)) {
            return created;
        }
        var types = GetConstructableTypes(type).ToArray();
        types.AssertAny($"No possible types found match requirement {type.GetNameCached()}");
        CreatedTypes[type] = types;
        return types;
    }
    public static Type CreateConstructableType(Type t) {
        if (Constructor.IsConstructableType(t)) {
            return t;
        }
        var validTypes = CreateConstructableTypes(t);
        return ChooseWeighted(validTypes);
    }
    public static void FillInMembers(object constructed) {
        if (constructed == null) {
            return;
        }
        Type constructedType = constructed.GetType();
        if (!Stream.IsAssemblyType(constructedType)) {
            return;
        }
        if (constructedType.IsEnum || constructedType.IsPrimitive) {
            return;
        }
        var classGenerator = constructedType.GetCustomAttributes<Generate>().SingleOrDefault();
        if (classGenerator == null) {
            // There's no generate attribute on the member, search for one in superclasses
            var searchHierarchy = true;
            classGenerator = constructedType.GetCustomAttributes<Generate>(searchHierarchy).FirstOrDefault();
        }
        if (classGenerator != null) {
            classGenerator.FillIn(constructed, constructedType);
        }
        foreach (var member in constructedType.GetMembers().OrderBy(GenerationOrder.GetOrder)) {
            var attributes = member.GetCustomAttributes();
            if (attributes.Any(x => x is HideInInspector)) {
                continue;
            }
            var generateAttribute = attributes.SingleOrDefault(x => typeof(Generate).IsAssignableFrom(x.GetType())) as Generate;
            if (generateAttribute != null) {
                generateAttribute.FillIn(constructed, member);
                continue;
            }
            if (member as PropertyInfo is var prop && prop != null) {
                var setterInfo = prop.GetSetMethod();
                if (setterInfo == null || setterInfo.GetParameters().Length != 1) {
                    // We didn't ask for private setter functions, so this is for either no setter or private setter
                    continue;
                }
                try {
                    var newValue = Generator.Generate(prop.PropertyType);
                    setterInfo.Invoke(constructed, new object[] { newValue });
                }
                catch {
                    Debug.LogError($"Couldn't fill in property {prop.GetNameCached()} in type {constructedType.GetNameCached()}");
                    throw;
                }
                continue;
            }
            if (member as FieldInfo is var field && field != null) {
                if (!field.IsPublic) {
                    continue;
                }
                try {
                    var newValue = Generator.Generate(field.FieldType);
                    field.SetValue(constructed, newValue);
                }
                catch {
                    Debug.LogError($"Couldn't fill in field {field.GetNameCached()} from type {member.DeclaringType.GetNameCached()} for type {constructedType.GetNameCached()}");
                }
                continue;
            }
        }
    }
    public static List<object> GetCreatedObjectsOfType(Type t) {
        if (!CreatedObjects.ContainsKey(t)) {
            CreatedObjects[t] = new List<object>();
        }
        return CreatedObjects[t];
    }
    public static IEnumerable<object> FindCreatedObjectsOfType(Type t) {
        foreach (var pair in CreatedObjects) {
            if (t.IsAssignableFrom(pair.Key)) {
                foreach (var createdObject in pair.Value) {
                    // No cast / generics here for the same reason as below
                    yield return createdObject;
                }
            }
        }
    }
    public static object Generate(Type t) {
        // We can't cast because Generate<List<ICastable<bool>>>() is supposed to return a List<SBool>
        var constructed = Constructor.Construct(CreateConstructableType(t));
        // Add it to the list of created objects before we fill in members, in case the members inject their parent
        GetCreatedObjectsOfType(t).Add(constructed);
        FillInMembers(constructed);
        return constructed;
    }
    public static object Generate(SerializedType t) {
        return Generate(t.Type);
    }
    public static object Generate<T>() {
        return Generate(typeof(T));
    }
    public static T GenerateTyped<T>() {
        var generated = Generate<T>();
        if (typeof(T).IsAssignableFrom(generated.GetType())) {
            return (T)generated;
        }
        throw new Exception($"Can't cast type {generated.GetType().GetNameCached()} to {typeof(T).GetNameCached()}");
    }
}

using System.Runtime.InteropServices.ComTypes;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PropertyVisitor {
    public delegate void VisitFN(object owner, PropertyInfo property, int? setArgument, object value);
    public delegate void PreVisitFN(object owner, PropertyInfo property, int? setArgument);
    public static bool VisitProperties(object visiting, VisitFN visitFn, PreVisitFN preVisitFN) {
        var success = true;
        if (visiting == null) {
            return success;
        }
        Type type = visiting.GetType();
        if (
            typeof(SObj).IsAssignableFrom(type) ||
            typeof(ScriptableSerializable).IsAssignableFrom(type)
        ) {
            foreach (var prop in type.GetPropertiesCached(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (prop.GetGetMethod(true)?.GetParameters().Length != 0 ||
                    prop.GetSetMethod(true)?.GetParameters().Length != 1)
                {
                    continue;
                }
                if (!prop.CanWrite) {
                    continue;
                }
                if (!typeof(ISerializable).IsAssignableFrom(prop.DeclaringType)) {
                    // This property is from a non-serializable ancestor class
                    continue;
                }
                if (prop.GetCustomAttributeCached(typeof(HideInInspector)) != null) {
                    continue;
                }
                preVisitFN(visiting, prop, null);
                object value = null;
                try {
                    value = prop.GetValue(visiting);
                }
                catch (Exception e) {
                    success = false;
                    var visitStr = "<toStringError>";
                    try {
                        visitStr = $"{visiting}";
                    }
                    // We can throw these errors away.  We're already in an error path, further errors are just confusing.
                    catch {}
                    var log = $"Encountered error while visiting property {prop} {visitStr}.  Expected during migration\n{e}";
                    if (Application.isPlaying) {
                        throw new Exception(log);
                    }
                    else {
                        Debug.LogWarning(log);
                    }
                }
                visitFn(visiting, prop, null, value);
                success &= VisitProperties(value, visitFn, preVisitFN);
            }
        }
        else if (typeof(SList<>).IsGenericAncestorOf(type)) {
            var slistType = type.GetTypeAncestor(typeof(SList<>));
            // This is apparently how you're supposed to get indexer properties?
            // https://docs.microsoft.com/en-us/dotnet/api/system.type.getproperty?view=netcore-3.1
            var indexerProp = slistType.GetPropertiesCached().Where(x => x.GetNameCached() == "Item" && x.PropertyType != typeof(object)).Single();
            var length = ((IList)visiting).Count;
            object[] arguments = new object[1];
            for (int i = 0; i < length; i++) {
                arguments[0] = i;
                preVisitFN(visiting, indexerProp, i);
                var value = indexerProp.GetValue(visiting, arguments);
                visitFn(visiting, indexerProp, i, value);
                success &= VisitProperties(value, visitFn, preVisitFN);
            }
        }
        return success;
    }

    public delegate void IndexedVisitFN(object owner, PropertyInfo property, ulong propertyIndex, int? setArgument, object value);
    public static void VisitIndexedProperties(object visiting, IndexedVisitFN visitFN) {
        var PropertyIndicies = Constructor.Construct<Dictionary<object, ulong>>();
        var PropertyChildCounts = Constructor.Construct<Dictionary<object, ulong>>();
        try {
            // This gets shifted off the end if we have a 4 layer deep index, and that's ok
            PropertyIndicies[visiting] = 0;
            PropertyChildCounts[visiting] = 0;
            // OK, so this ended up getting really complicated, so bear with me
            // Originally, propertyIndex was just a simple index into a dfs tree of properties
            // That's really fragile because you can't change the number of properties on any object
            // So now, the 64-bit "index" is broken up into 16 4-bit chunks
            // Any object in a skill tree can have no more than 15 children
            // And no object can have nesting deeper than 16 layers
            // To preserve the "low indexes correspond to early properties" property, parent ids get shifted higher for children
            // So the "action" property is 1 (not 0 for reasons), and the action property's first subprop is 0x0101
            // We start the indicies for properties at 1, because otherwise 0 would be ambiguous
            PropertyVisitor.VisitProperties(visiting, (owner, property, setArgument, value) => {
                if (property.GetCustomAttributesCached().Any(x => x is Generate || x is HideInInspector)) {
                    // ATM, we only use the indexed visitor for upgrade intervention stuff, which doesn't want these
                    return;
                }
                var parentIndex = PropertyIndicies[owner];
                // We technically only need the +1 before we calculate our index on the first layer
                // I'll wait for that to be a problem before solving it
                var childNum = PropertyChildCounts[owner] + 1;
                if (childNum > 0xf) {
                    Assert.That(false, $"Too many children for {owner}");
                }
                Assert.That(parentIndex < 0x1000000000000000, $"Nesting too deep!");
                var propertyIndex = (parentIndex << 4) + childNum;
                if (value != null) {
                    PropertyIndicies[value] = propertyIndex;
                    PropertyChildCounts[value] = 0;
                }
                PropertyChildCounts[owner] = childNum;
                visitFN(owner, property, propertyIndex, setArgument, value);
            }, (owner, property, setArgument) => { });
        }
        finally {
            PropertyIndicies.Clear();
            PropertyChildCounts.Clear();
            Pool.Return(PropertyIndicies);
            Pool.Return(PropertyChildCounts);
        }
    }
}
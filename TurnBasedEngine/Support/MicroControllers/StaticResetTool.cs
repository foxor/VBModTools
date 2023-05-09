using System.Linq;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectStaticsAttribute : Attribute {
}

[ProtectStatics]
public class StaticResetTool {
    protected static IEnumerable<Type> GetDomainTypes() {
        // This is intentionally different from the "derives from IAssemblyType" criteria from Stream
        // This is intended to wipe values from any old POJO, not just those that can serialize
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            // This matches all of the assemblies unity makes for user code, and none of the system ones.
            if (!assembly.FullName.Contains("CSharp")) {
                continue;
            }
            foreach (Type t in assembly.GetTypes()) {
                if (t.ContainsGenericParameters) {
                    continue;
                }
                if (t.CustomAttributes.Any(x => x.AttributeType == typeof(ProtectStaticsAttribute))) {
                    continue;
                }
                yield return t;
            }
        }
    }

    protected static Type[] domainTypes;
    public static Type[] DomainTypes {
        get {
            if (domainTypes == null) {
                domainTypes = GetDomainTypes().ToArray();
            }
            return domainTypes;
        }
    }

    public static void Reset() {
        foreach (Type t in DomainTypes) {
            foreach (var field in t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)) {
                // IsAssembly == internal
                if (!field.IsLiteral && !field.IsInitOnly && !field.Name.Equals("m_AllowMultiObjectAccess")) {
                    field.SetValue(null, null);
                }
            }
        }
    }
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TypeExtension
{
    public static bool MatchesTypeParameter(this Type type, Type typeParameter)
    {
        if (!typeParameter.IsGenericParameter)
        {
            return false;
        }
        while (type != null && type != typeof(object))
        {
            if (type.IsAssignableFrom(typeParameter))
            {
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }
    public static bool IsGenericAncestorOf(this Type genericAncestor, Type child) {
        if (child == null) {
            return false;
        }
        if (child.IsGenericType && child.GetGenericTypeDefinition() == genericAncestor) {
            return true;
        }
        return genericAncestor.IsGenericAncestorOf(child.BaseType);
    }

    public static Type GetTypeAncestor(this Type t, Type AncestorType) {
        if (t == null) {
            return null;
        }
        Assert.That(AncestorType.IsGenericTypeDefinition);
        // We're going to walk the baseType, so interfaces won't work
        Assert.That(!AncestorType.IsInterface);
        if (t.IsGenericType && t.GetGenericTypeDefinition() == AncestorType) {
            return t;
        }
        return t.BaseType.GetTypeAncestor(AncestorType);
    }

    public static Type GetGenericInterface(this Type type, Type InterfaceType) {
        if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == InterfaceType) {
            return type;
        }
        return type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == InterfaceType).SingleOrDefault();
    }
}
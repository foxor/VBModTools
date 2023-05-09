using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InjectAll : Inject {
    public Type iterationType;
    public InjectAll(Type iterationType) {
        this.iterationType = iterationType;
    }

    public override object GetFillValue(Type t) {
        Type listType = typeof(List<>).MakeGenericType(t.GetGenericArguments());
        var value = Constructor.Construct(listType);
        foreach (var obj in Generator.GetCreatedObjectsOfType(iterationType)) {
            (value as IList).Add(obj);
        }
        return value;
    }
}
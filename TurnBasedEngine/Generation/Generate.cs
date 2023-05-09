using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public abstract class Generate : Attribute {
    public abstract void FillIn(object obj, MemberInfo memberInfo);
    public static Type GetMemberType(MemberInfo memberInfo) {
        if (memberInfo is PropertyInfo) {
            return (memberInfo as PropertyInfo).PropertyType;
        }
        if (memberInfo is FieldInfo) {
            return (memberInfo as FieldInfo).FieldType;
        }
        if (memberInfo is Type) {
            return memberInfo as Type;
        }
        throw new Exception($"Tried to get the type of a non-field, non-property!  MemberInfo is {memberInfo.GetType()}");
    }
    public static void SetMember(object obj, MemberInfo memberInfo, object value) {
        if (memberInfo is PropertyInfo) {
            try {
                (memberInfo as PropertyInfo).SetValue(obj, value);
            }
            catch {
                throw;
            }
        }
        else if (memberInfo is FieldInfo) {
            (memberInfo as FieldInfo).SetValue(obj, value);
        }
        else {
            throw new Exception($"Tried to set the value of a non-field, non-property!  MemberInfo is {memberInfo.GetType()}");
        }
    }
    public static object GetMember(object obj, MemberInfo memberInfo) {
        if (memberInfo is PropertyInfo) {
            return (memberInfo as PropertyInfo).GetValue(obj);
        }
        else if (memberInfo is FieldInfo) {
            return (memberInfo as FieldInfo).GetValue(obj);
        }
        else {
            throw new Exception($"Tried to set the value of a non-field, non-property!  MemberInfo is {memberInfo.GetType()}");
        }
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GenerateObject : Generate {
    public override void FillIn(object obj, MemberInfo memberInfo) {
        Type memberType = GetMemberType(memberInfo);
        var value = Generator.Generate(memberType);
        SetMember(obj, memberInfo, value);
    }
}
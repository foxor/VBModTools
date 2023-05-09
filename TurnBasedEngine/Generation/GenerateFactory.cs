using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public class GenerateFactory : Generate {
	protected string factoryFunctionName;

	// You can't pass in a func to an attribute, apparently
	public GenerateFactory(string factoryFunctionName) {
		this.factoryFunctionName = factoryFunctionName;
	}

    public override void FillIn(object obj, MemberInfo memberInfo) {
		Type objType = obj.GetType();
		MethodInfo factoryInfo = objType.GetMethod(factoryFunctionName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		var value = factoryInfo.Invoke(obj, new object[] {});
        SetMember(obj, memberInfo, value);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GenerationOrder : Attribute {
	protected int Order;
	public GenerationOrder(int Order) {
		this.Order = Order;
	}

	public static int GetOrder(MemberInfo member) {
		var orderAttribute = member.GetCustomAttributes<GenerationOrder>().FirstOrDefault();
		return orderAttribute == null ? 100 : orderAttribute.Order;
	}
}

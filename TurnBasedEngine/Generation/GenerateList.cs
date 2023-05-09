using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public class GenerateList : Generate {
	public int Elements = 0;

	public GenerateList() {}
	public GenerateList(int Elements) {
		this.Elements = Elements;
	}
    public override void FillIn(object obj, MemberInfo memberInfo) {
        Type memberType = GetMemberType(memberInfo);
		Type listElementType = memberType.GetGenericInterface(typeof(IEnumerable<>)).GetGenericArguments().Single();
		Type listType;
		if (memberType.GetTypeAncestor(typeof(SList<>)) != null) {
			listType = memberType;
		}
		else {
			// If it's List<T>, this does nothing, if it's IEnumerable<T>, it makes a list to fill in
			listType = typeof(List<>).MakeGenericType(listElementType);
		}
		object value;
		var memberInfoIsType = typeof(Type).IsAssignableFrom(memberInfo.GetType());
		if (memberInfoIsType) {
			// Rather than building a list and assigning it to a property/field, we just call the add method on the object
			value = obj;
		}
		else {
			value = Constructor.Construct(listType);
		}
		var addMethod = listType.GetMethods().Where(method => 
			method.GetNameCached() == "Add" &&
			method.GetParameters()[0].ParameterType.IsAssignableFrom(listElementType)
		).Single();
		foreach (var i in Enumerable.Range(0, Elements)) {
			addMethod.Invoke(value, new object[]{ Generator.Generate(listElementType) });
		}
		if (!memberInfoIsType) {
			// We already filled this in is memberInfoIsType
        	SetMember(obj, memberInfo, value);
		}
    }
}
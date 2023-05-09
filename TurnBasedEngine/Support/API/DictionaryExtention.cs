using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryExtention
{
	public static V Get<K, V>(this Dictionary<K, V> dict, K key, V defaultValue = null) where V : class
	{
		V value = null;
		if (dict.TryGetValue(key, out value)) {
			return value;
		}
		return defaultValue;
	}

	public static void SetCombine<K, V>(this Dictionary<K, V> dict, K key, V value, Func<V, V, V> combine) where V : class {
		V existingValue = null;
		if (dict.TryGetValue(key, out existingValue)) {
			dict[key] = combine(existingValue, value);
		}
		else {
			dict[key] = value;
		}
	}
}
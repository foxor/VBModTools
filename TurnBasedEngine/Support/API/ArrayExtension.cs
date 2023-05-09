using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ArrayExtension {
	public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) {
		foreach (IEnumerable<T> subEnumerable in source) {
			foreach (T element in subEnumerable) {
				yield return element;
			}
		}
	}
	public static IEnumerable<T> Union<T>(this IEnumerable<IEnumerable<T>> source) {
		if (!source.Any()) {
			return new T[]{};
		}
		var unionSet = source.First();
		foreach (var subEnumerable in source.Skip(1)) {
			unionSet = unionSet.Union(subEnumerable);
		}
		return unionSet;
	}
	public static IEnumerable<T> Intersect<T>(this IEnumerable<IEnumerable<T>> source) {
		if (!source.Any()) {
			return new T[]{};
		}
		var intersectSet = source.First();
		foreach (var subEnumerable in source.Skip(1)) {
			intersectSet = intersectSet.Intersect(subEnumerable);
		}
		return intersectSet;
	}
	public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
		int index = 0;
		foreach (T elem in source) {
			if (predicate(elem)) {
				return index;
			}
			index++;
		}
		return -1;
	}
	public static int IndexOf<T>(this IEnumerable<T> source, T predicate) {
		int index = 0;
		foreach (T elem in source) {
			if (ReferenceEquals(elem, predicate)) {
				return index;
			}
			index++;
		}
		return -1;
	}
}
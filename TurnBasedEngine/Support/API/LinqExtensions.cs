using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LinqExtension
{
    public static void AssertAny<T>(this IEnumerable<T> source, string message) {
        Assert.That(source.Any(), 2, message);
    }

    public static bool IdenticalContents<T>(this IEnumerable<T> a, IEnumerable<T> b) {
        return a.Count() == b.Count() && !a.Any(x => !b.Contains(x));
    }

    public static IEnumerable Replace<T>(this IEnumerable<T> a, Func<T, bool> predicate, T replacement) {
        return a.Select(x => predicate(x) ? replacement : x);
    }

    public static SList<T> ToSList<T>(this IEnumerable<T> a) where T : ISerializable {
        return new SList<T>(a.ToList());
    }

    public static IEnumerable<int> IndiciesWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        int index = 0;
        foreach (var t in source) {
            if (predicate(t)) {
                yield return index;
            }
            index++;
        }
    }

    public static IEnumerable<T> ExceptIndicies<T>(this IEnumerable<T> source, int[] indicies) {
        var index = 0;
        foreach (var elem in source) {
            if (!indicies.Contains(index)) {
                yield return elem;
            }
            index++;
        }
    }

    public static IEnumerable<T> OnlyIndicies<T>(this IEnumerable<T> source, int[] indicies) {
        var index = 0;
        foreach (var elem in source) {
            if (indicies.Contains(index)) {
                yield return elem;
            }
            index++;
        }
    }
    
    // The goal here is to return all combinations of inputs, so: [[a,b],[c,d]] -> [[a,c],[b,d],[a,d],[b,c]]
    // The length of the first array in the input is the length of the nested array in the output, for example: [[a],[b],[c],[d, e]] -> [[a,b,c,e],[a,b,c,d]]
    public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<IEnumerable<T>> source) {
        var sourceArrays = source.Select(x => x.ToArray());
        int count = sourceArrays.Aggregate(1, (a, x) => a * x.Length);
        for (; count > 0; count--) {
            int captureCopy = count;
            yield return sourceArrays.Select(x => {
                int index = captureCopy % x.Length;
                captureCopy /= x.Length;
                return x[index];
            });
        }
    }

    public static T Index<T>(this IEnumerable<T> source, int index) {
        return source.Skip(index).FirstOrDefault();
    }

    public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        foreach (var x in source) {
            if (predicate(x)) {
                return true;
            }
        }
        return false;
    }
}
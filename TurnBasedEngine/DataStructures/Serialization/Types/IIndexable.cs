using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIndexable<RootType> : IEnumerable<RootType> {
    void Add<T>(T element) where T : ISerializable, RootType;
    RootType this[int index] { get; set; }
    IEnumerable<RootType> GetEnumerable();
    int Count { get; }
}
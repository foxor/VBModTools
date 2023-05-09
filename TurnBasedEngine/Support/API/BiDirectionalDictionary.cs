using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiDirectionalDictionary <T1, T2>
{
    private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

    public BiDirectionalDictionary()
    {
        this.Forward = new Indexer<T1, T2>(_forward);
        this.Reverse = new Indexer<T2, T1>(_reverse);
    }

    public class Indexer<T3, T4>
    {
        private Dictionary<T3, T4> _dictionary;
        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }
        public T4 this[T3 index]
        {
            get { return _dictionary[index]; }
        }
    }

    public void Add(T1 t1, T2 t2)
    {
        _forward.Add(t1, t2);
        _reverse.Add(t2, t1);
    }

    public bool Remove(T1 t1)
    {
        T2 var = default(T2);
        if (_forward.TryGetValue(t1, out var)) {
            _reverse.Remove(var);
            _forward.Remove(t1);
            return true;
        }
        return false;
    }
    public T2 this[T1 index]
    {
        get { return Forward[index]; }
        set { _forward[index] = value; _reverse[value] = index; }
    }

    public bool ContainsKey(T1 t1) {
        return _forward.ContainsKey(t1);
    }

    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Reverse { get; private set; }
}
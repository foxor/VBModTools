using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Pair <K, V> {
    public K Key;
    public V Value;
}

public abstract class EnumMetadata<S, P, K, V> : SingletonScriptableObject<S>
    where S : EnumMetadata<S, P, K, V>
    where P : Pair<K, V>
    where K : System.Enum
{
    // This is stupid and I hate it.  Thanks UNITY
    [SerializeField]
    private List<P> SerializedDict = new List<P>();

    private Dictionary<K, V> _Dict;
    private Dictionary<K, V> Dict {
        get {
            if (_Dict == null) {
                _Dict = SerializedDict.ToDictionary(x => x.Key, x => x.Value);
            }
            return _Dict;
        }
    }

    public IEnumerable<K> Keys { get => Dict.Keys; }
    public IEnumerable<V> Values { get => Dict.Values; }

    public bool TryGetValue(K key, out V value) {
        return Dict.TryGetValue(key, out value);
    }

    public V this[K key] {
        get {
            if (Dict.TryGetValue(key, out var r)) {
                return r;
            }
            throw new Exception($"Enum metadata for {typeof(K)} - {name} is missing a required value {key}");
        }
    }
}

public abstract class EnumScriptableMetadata<S, P, K, V> : EnumMetadata<S, P, K, ScriptableSerializable>
    where S : EnumMetadata<S, P, K, ScriptableSerializable>
    where P : Pair<K, ScriptableSerializable>
    where V : ISerializable
    where K : System.Enum
{
    public new V this[K key] {
        get {
            return (V)base[key].DecodedValue;
        }
    }
}
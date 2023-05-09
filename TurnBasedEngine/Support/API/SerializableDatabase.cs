using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SerializableDatabase<T, S> : SingletonScriptableObject<S> where S : ScriptableObject where T : ISerializable
{
    public List<ScriptableSerializable> Database;
    protected T[] cache;
    public IEnumerable<T> GetValues()
    {
        if (cache == null) {
            cache = Database.Select(x => (T)x.DecodedValue.DeepCopy()).ToArray();
        }
        return cache;
    }
}
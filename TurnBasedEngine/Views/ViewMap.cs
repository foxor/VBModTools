using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ViewMap : SingletonScriptableObject<ViewMap>
{
    [Serializable]
    public class TypePair : Pair<SerializedType, GameObject> {}
    [SerializeField]
    private List<TypePair> SerializedDict = new List<TypePair>();
    
    private Dictionary<Type, GameObject> _TypeToPrefab;
    private Dictionary<Type, GameObject> TypeToPrefab {
        get {
            if (_TypeToPrefab == null) {
                _TypeToPrefab = SerializedDict.ToDictionary(x => x.Key.Type, x => x.Value);
            }
            return _TypeToPrefab;
        }
    }

    private GameObject GetViewTypePolymorphic(Type type) {
        var prefab = TypeToPrefab.Get(type);
        if (prefab == null && type.BaseType != null) {
            // This could easily support interfaces too
            return GetViewTypePolymorphic(type.BaseType);
        }
        return prefab;
    }

    public GameObject this[Type type] {
        get {
            var prefab = GetViewTypePolymorphic(type);
            if (prefab == null) {
                Assert.That(false, $"No view registered for {type.GetNameCached()}");
            }
            return prefab;
        }
    }

    public void Remove(Type type) {
        SerializedDict = SerializedDict.Where(x => x.Key.Type != type).ToList();
        _TypeToPrefab = null;
    }
}

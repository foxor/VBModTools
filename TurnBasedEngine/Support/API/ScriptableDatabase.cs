using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableDatabase<T, S> : SingletonScriptableObject<S> where S : ScriptableObject
{
    public List<T> Database;
}
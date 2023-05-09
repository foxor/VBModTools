using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DependentBehavior<T> : MonoBehaviour where T : Component {
    protected T IndependentComponent { get; private set; }
    public virtual void Awake() {
        IndependentComponent = GetComponent<T>();
    }
}
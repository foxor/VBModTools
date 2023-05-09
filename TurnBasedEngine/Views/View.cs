using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IView {}

public interface IModelView<T> : IView
    where T : Model
{
    void Setup(in T model);
}

public abstract class View<T> : MonoBehaviour, IDisposable, IModelView<T>, IPoolable
    where T : Model
{
    public int modelId;
    protected T Model => (T)ModelViewController.Instance.GetViewModel(modelId);
    protected Dependency dependency { get; private set; }
    private List<(int, ModelViewController.RegistrationClosure)> Registrations = new List<(int, ModelViewController.RegistrationClosure)>();
    protected void Register<A>(int key, T model, ModelViewController.Registration<A> registration) where A : ISerializable {
        Assert.That(!Registrations.Any(x => x.Item1 == key), $"This view has registered multiple listeners for {key}");
        Registrations.Add((key, ModelViewController.Instance.AddRegistration(model, key, registration)));
    }
    public virtual void Setup(in T model) {
        modelId = model.Id;
        dependency = model.GetDependencies();
    }
    // This should be called either when disposed or returned
    public virtual void Teardown() {
        foreach (var key in Registrations) {
            ModelViewController.Instance.ClearRegistration(modelId, key.Item1, key.Item2);
        }
        Registrations.Clear();
    }
    public virtual void Return() {
        Teardown();
        if (this == null) {
            return;
        }
        if (Pool.Return(this)) {
            transform.SetParent(Tombstone.Instance.transform);
        }
        else {
            if (this != null && gameObject != null) {
                GameObject.Destroy(gameObject);
            }
        }
    }
    public virtual void Dispose() {
        Teardown();
        if (this != null && gameObject != null) {
            GameObject.Destroy(gameObject);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class ModelViewController {
    // This is the same struct that's packed into a ulong in the context of ILatent, but there's no reason to change it here, since these systems don't intersect
    public struct ModelRelationShip {
        public int ModelId;
        public int UpdateKey;
        public ModelRelationShip(int ModelId, int UpdateKey) {
            this.ModelId = ModelId;
            this.UpdateKey = UpdateKey;
        }
    }
    protected static ModelViewController instance;
    public static ModelViewController Instance {
        get {
            if (instance == null) {
                instance = new ModelViewController();
                SceneController.OnSceneChanged += instance.Cleanup;
            }
            return instance;
        }
    }
    public delegate ILock Registration<A>(in A dataCopy) where A : ISerializable;
    public delegate ILock RegistrationClosure(ISerializable dataCopy);
    protected List<List<IView>> ModelViews = new List<List<IView>>();
    protected List<Dictionary<int, RegistrationClosure>> UpdateFunctions = new List<Dictionary<int, RegistrationClosure>>();
    protected Dictionary<object, ModelRelationShip> SerializableToModel = new Dictionary<object, ModelRelationShip>();
    protected List<ModelRelationShip> ModelToSuperModel = new List<ModelRelationShip>();
    protected List<Model> SourceModels = new List<Model>();
    protected List<Model> SinkModels = new List<Model>();
    protected void ChangeSinkModel(ModelRelationShip relationship, ISerializable newValue = null) {
        var model = SinkModels[relationship.ModelId];
        if (UpdateFunctions[relationship.ModelId].TryGetValue(relationship.UpdateKey, out var updateFunctions) == true && relationship.UpdateKey >= 0) {
            var snapshotValue = newValue;
            if (snapshotValue == null) {
                snapshotValue = model.GetRaw(relationship.UpdateKey);
            }
            foreach (RegistrationClosure updateFunction in updateFunctions.GetInvocationList()) {
                LockController.Instance.Request(updateFunction(snapshotValue));
            }
        }

        if (newValue != null) {
            model.SetRaw(relationship.UpdateKey, newValue);
        }

        if (GetSuperModel(relationship.ModelId, out var grandRelationship)) {
            OnModelChanged(grandRelationship);
        }
    }
    protected void OnModelChanged(ModelRelationShip relationship, ISerializable newValue = null) {
        if (TestDetector.AreTestsActive) {
            return;
        }
        if (SNull.IsNull(SinkModels[relationship.ModelId])) {
            // Sink model isn't ready yet
            Action OnceViewReady = () => {
                ChangeSinkModel(relationship, newValue);
            };
            LockController.Instance.Request(new ModelCallbackReadLock(SourceModels[relationship.ModelId].GetDependencies(), "Waiting for model to be created to pump event", OnceViewReady));
        }
        else {
            ChangeSinkModel(relationship, newValue);
        }
    }
    protected RegistrationClosure GenerateClosureFactory<A>(Registration<A> registration) where A : ISerializable {
        return (ISerializable newValue) => {
            // Future Isaac: The reason this has to be like this is so that the "simulation thread" can go off
            // and do whatever it wants to do, and the UI can get a queue of updates.  This pattern lets the UI turn events into
            // locks, which can animate at their own pace, while presenting an accurate snapshot into the past.
            // The UI may not depend on the referential integrity of the model supplied, as it will not be preserved.
            Assert.That(newValue is A || SNull.IsNull(newValue));//, $"Registration required type {typeof(A)} but property of was of type {copy.GetType()}");
            var typedCopy = (A)newValue;
            var lockValue = registration(in typedCopy);
            return lockValue;
        };
    }
    public bool GetModelRelationship(out ModelRelationShip relationship, ISerializable serializable, int key = -1) {
        relationship.ModelId = -1;
        relationship.UpdateKey = key;
        if (TestDetector.AreTestsActive) {
            return false;
        }
        if (serializable == null) {
            return false;
        }
        if (serializable as Model is var model && model != null) {
            if (model.Destroied) {
                return false;
            }
            if (key == 0) {
                //This is the id getting added right now
                return false;
            }
            if (!model.HasId) {
                // This has the potential to cause some recurrence problems.
                // The reason we need to this is because some models are being created outside the game,
                // like boss skills, so we may "discover" that they're models during gameplay.

                // *** This code is on probation.  If you're looking at this again, it's time to find a real solution ***
                model.Setup();
            }
            relationship.ModelId = model.Id;
            return true;
        }
        var hasKey = SerializableToModel.TryGetValue(serializable, out relationship);
        if (!hasKey) {
            relationship.ModelId = -1;
            return false;
        }
        return true;
    }
    public IEnumerable<int> GetAncestorsInner(int? modelId) {
        if (modelId == -1) {
            yield break;
        }
        var ancestor = modelId;
        while (ancestor.HasValue) {
            yield return ancestor.Value;
            if (!GetSuperModel(ancestor.Value, out var grandRelationShip)) {
                break;
            }
            ancestor = grandRelationShip.ModelId;
        }
    }
    public void OnSerializableLinked(ISerializable parent, ISerializable newChild, int key = -1) {
        // This is performance sensitive, so asserts text formatting is manually disabled
        if (TestDetector.AreTestsActive) {
            return;
        }
        if (newChild == null) {
            return;
        }
        ModelRelationShip relationship;
        var hasParentModel = GetModelRelationship(out relationship, parent, key);
        if (!SerializableToModel.ContainsKey(newChild)) {
            // If it's already linked to something else, that's unsupported.
            // We're supposed to go into this block IFF it's a new thing.
            if (hasParentModel) {
                // We are potentially a subproperty, in which case we want to use the primary property's key
                SerializableToModel.Add(newChild, relationship);
            }
            if (newChild is Model) {
                var childModel = (Model)newChild;
                if (!childModel.HasId) {
                    childModel.Setup();
                }
                else {
                    Assert.That(SourceModels[childModel.Id] == childModel);//, $"Model {childModel} has ID {childModel.Id}, but that id belongs to {Models[childModel.Id]}");
                }
                var id = childModel.Id;
                if (hasParentModel) {
                    Assert.That(id < ModelToSuperModel.Count);//, $"{newChild} has an invalid model ID.  Is it serialized?");
                    ModelToSuperModel[id] = relationship;
                }
            }
        }
        if (hasParentModel) {
            if (newChild is Model) {
                Assert.That(relationship.ModelId != (newChild as Model).Id);
            }
            OnModelChanged(relationship);
        }
    }
    public void PreSerializableUnlinked(ISerializable parent, ISerializable oldChild, int key = -1) {
        if (TestDetector.AreTestsActive) {
            return;
        }
        if (ReferenceEquals(oldChild, null)) {
            return;
        }
        // This function used to walk the dependency tree in both directions and kill everyone, but that was a performance problem.
        if (oldChild is Model model) {
            OnModelDestroyed(model);
        }
    }
    public void OnSerializableRemoved(ISerializable parent, int key) {
        if (GetModelRelationship(out var relationship, parent, key)) {
            // Maybe the member info should be packaged in a RESTlike package to say this is a delete, but that may be clear from context.
            OnModelChanged(relationship);
        }
    }
    // Disassocate model initialization from view creation.
    // Some models should have 0 views, some should have many
    public void OnModelCreated(Model model) {
        if (TestDetector.AreTestsActive) {
            return;
        }
        var id = SourceModels.Count;
        Assert.That(!model.HasId);//, $"Tried to set up a {model.GetType().Name} twice!  Old Id is {model.Id}");
        model.Set<SInt>(0, id);
        SourceModels.Add(model);
        SinkModels.Add(null);
        UpdateFunctions.Add(new Dictionary<int, RegistrationClosure>());
        ModelViews.Add(new List<IView>());
        ModelToSuperModel.Add(new ModelRelationShip(-1, -1));
    }
    [Conditional("ASSERTS_ENABLED")]
    public void VerifyModelIdCorrect(Model model) {
        Assert.That(System.Object.ReferenceEquals(SourceModels[model.Id], model), $"ID verification failed!  {model.GetType().GetNameCached()} and {SourceModels[model.Id].GetType().GetNameCached()} share an ID!");
    }
    public void CreateView<V, M>(M model, Action<V> callback = null) where V : IModelView<M> where M : Model {
        if (TestDetector.AreTestsActive) {
            return;
        }
        if (!model.HasId) {
            OnModelCreated(model);
        }
        SinkModels[model.Id] = model.DeepCopy();
        var capturedId = model.Id.Value;
        Action createFn = () => {
            var view = Constructor.Construct<V>();
            Assert.That(view != null, $"Type {SinkModels[capturedId].GetType().GetNameCached()} has a view in the viewMap that does not contain a {typeof(V).GetNameCached()} typed view coponent");
            AddView((M)SinkModels[capturedId], view);
            if (callback != null) {
                callback(view);
            }
        };
        var writeLock = true;
        LockController.Instance.Request(new ModelCallbackLock(model.GetDependencies(), $"Creating a {typeof(V).GetNameCached()} view for {model}", createFn, writeLock));
    }
    protected void AddView<V, M>(M model, V view) where V : IModelView<M> where M : Model {
        if (!model.HasId || ModelViews[model.Id] == null) {
            // The model had an ID when we requested the lock.  It's been destroyed since then, which happens in Model.Dispose
            DestroyView(view);
            return;
        }
        ModelViews[model.Id].Add(view);
        view.Setup(model);
    }
    public void RemoveViewsOfType<V>(Model model) {
        RemoveFilteredViews(model, v => v is V);
    }
    public void RemoveFilteredViews<M>(M model, Func<IView, bool> filter) where M : Model {
        if (TestDetector.AreTestsActive || model == null || filter == null) {
            return;
        }
        int capturedId = model.Id;
        Action searchAndDestroy = () => {
            if (ModelViews[capturedId] == null) {
                return;
            }
            foreach (var view in ModelViews[capturedId].Where(filter).ToArray()) {
                DestroyView(view);
                ModelViews[capturedId].Remove(view);
            }
        };
        LockController.Instance.Request(new ModelCallbackLock(model.GetDependencies(), $"Destroying a view for {model}", searchAndDestroy, true));
    }
    private void DestroyView(IView view) {
        if (view is IPoolable poolable) {
            poolable.Return();
        }
        else if (view is IDisposable disposable) {
            disposable.Dispose();
        }
    }
    private void OnModelDestroyed<T>(T model) where T : Model {
        if (TestDetector.AreTestsActive) {
            return;
        }
        if (model?.HasId != true) {
            return;
        }
        Assert.That(model.Id <= ModelViews.Count && model.Id >= 0);
        int capturedId = model.Id;
        Action DestroyFinalize = () => {
            if (ModelViews[capturedId] != null) {
                foreach (var view in ModelViews[capturedId]) {
                    DestroyView(view);
                }
            }
            ModelViews[capturedId] = null;
            // If there are huge gaps in here eventually, we'll need to re-use ids, or switch back to a dictionary
            SourceModels[capturedId] = null;
            SinkModels[capturedId] = null;
            UpdateFunctions[capturedId] = null;
            ModelToSuperModel[capturedId] = new ModelRelationShip(-1, -1);
        };
        var writeLock = true;
        LockController.Instance.Request(new ModelCallbackLock(model.GetDependencies(), $"Destroying {model}", DestroyFinalize, writeLock));
    }
    public RegistrationClosure AddRegistration<T, A>(in T model, int key, Registration<A> registration) where T : Model where A : ISerializable {
        var newRegistration = GenerateClosureFactory(registration);
        if (!UpdateFunctions[model.Id].ContainsKey(key)) {
            UpdateFunctions[model.Id][key] = newRegistration;
        }
        else {
            UpdateFunctions[model.Id][key] += newRegistration;
        }
        // This is better than calling OnModelChanged for a couple of reasons:
        //  1) This is called from view code, and we shouldn't pretend that the copy of a model that just came through the lock pipeline is up to date
        //  2) That strategy calls ALL the registrations for that key, including those that weren't just added, resulting mu multiple unecessary calls
        if (model.HasProperty(key)) {
            // We don't wait for a lock for this, since this is the "initialization" of this data, and we already waited in line to create the object
            // We don't want the object to sit around uninitialized while the queue drains
            registration((A)model.GetRaw(key))?.Enter(null);
        }
        return newRegistration;
    }
    public void ClearRegistration(int modelId, int key, RegistrationClosure registration) {
        if (UpdateFunctions[modelId] == null) {
            return;
        }
        UpdateFunctions[modelId][key] -= registration;
        if (UpdateFunctions[modelId][key] == null) {
            UpdateFunctions[modelId].Remove(key);
        }
    }
    protected bool GetSuperModel(int subModelId, out ModelRelationShip relationship) {
        relationship = ModelToSuperModel[subModelId];
        return relationship.ModelId != -1;
    }
    public List<IView> GetViews<T>(T model) where T : Model {
        return ModelViews[model.Id];
    }
    public Model GetViewModel(ISerializable property) {
        if (GetModelRelationship(out var relationship, property)) {
            return SinkModels[relationship.ModelId];
        }
        return null;
    }
    public Model GetViewModel(int modelId) {
        return SinkModels[modelId];
    }
    private void Cleanup() {
        foreach (var model in SourceModels) {
            if (SNull.IsNull(model)) {
                continue;
            }
            // We don't want model IDs to outlast the MVC structures for them, since they point to freed indicies.
            // Most models get cleaned up nicely, but save state exists outside of the main view hierarchy and survives scene changes.
            model.Id = null;
        }
    }
}

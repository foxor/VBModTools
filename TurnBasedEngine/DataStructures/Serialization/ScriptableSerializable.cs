using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableSerializable : ScriptableObject {
    public static Action OnError;
    public string EncodedSerializable;
    public int Version;

    // These values are persisted in a static to force them to reload on assembly reload
    // When the active assembly doesn't match the assembly they were loaded on, type equality fails, which causes huge problems
    protected static Dictionary<object, ISerializable> decodedValues;
    protected static Dictionary<object, ISerializable> DecodedValues {
        get {
            if (decodedValues == null) {
                decodedValues = new Dictionary<object, ISerializable>();
            }
            return decodedValues;
        }
    }
    [HideInInspector]
    public bool BlockSave;

    protected void VisitFN(object owner, PropertyInfo property, int? setArgument, object value) {
        if (property.GetCustomAttributeCached(typeof(HideInInspector)) != null) {
            if (value != null) {
                Debug.LogWarning($"{owner} has a {property.GetNameCached()} property serialized, which is not supposed to be serialized");
            }
            value = null;
            if (setArgument.HasValue) {
                property.SetValue(owner, null, new object[] { setArgument.Value });
            }
            else {
                property.SetValue(owner, null);
            }
        }
    }

    protected void PreVisitFN(object owner, PropertyInfo property, int? setArgument) {
        DataVersionController.EnsureCurrent(owner, property, setArgument, Version);
    }

    public ISerializable DecodedValue {
        get {
            ISerializable decodedValue;
            if (!DecodedValues.TryGetValue(this, out decodedValue)) {
                if (EncodedSerializable != null) {
                    try {
                        decodedValue = new Stream(System.Convert.FromBase64String(EncodedSerializable), Version).Consume();
                    }
                    catch {
                        Debug.LogWarning("Failed to decode scriptable serializable: " + this.name);
                        throw;
                    }

                    // This data migration model assumes that the basic chunks of serialization (SBool, SList, SInt, SString, SNull, etc)
                    // all never change.  It's usually the classes built on top (SSkill, CharacterInitializationData, etc) that change.
                    // Therefore, we just load a valid SList, and make changes to it before attempting to interpert it as a higher
                    // level structure.
                    // If I want to change how lists work or something, I'll have to add version data to the stream above.
                    if (Version != DataVersionController.CURRENT_DATA_VERSION) {
                        Debug.LogWarning($"Upgrading data version on {name} from {Version} to {DataVersionController.CURRENT_DATA_VERSION}");
                    }
                    try {
                        var success = PropertyVisitor.VisitProperties(decodedValue, VisitFN, PreVisitFN);
                        BlockSave = !success;
                        if (!success && OnError != null) {
                            OnError();
                        }
                        if (success) {
                            Version = DataVersionController.CURRENT_DATA_VERSION;
                        }
                    }
                    catch (Exception e) {
                        throw new Exception($"Tried to visit properties of {name}, but encountered error:", e);
                    }

                    DecodedValues[this] = decodedValue;
                }
            }
            return decodedValue;
        }
        set {
            if (BlockSave) {
                Debug.LogWarning($"Blocking save of {name} due to previous error");
                return;
            }
            DecodedValues[this] = value;
            Assert.That(TestDetector.AreTestsActive, $"Attempting to set value {name} into scriptable object at runtime!!!");
            EncodedSerializable = DecodedValueAsString;
        }
    }

    public string DecodedValueAsString {
        get {
            return System.Convert.ToBase64String(DecodedValue.ToStream().data);
        }
    }
}
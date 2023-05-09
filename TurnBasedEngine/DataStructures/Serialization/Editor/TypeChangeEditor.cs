using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class TypeChangeEditor : EditorWindow {
    public string oldName;
    public string newName;
    public int dataVersion = DataVersionController.CURRENT_DATA_VERSION;
    public static int errorCount;
    protected int addCount;

    [MenuItem("Tools/Serializaed Type Name Changer")]
    protected static void Init() {
        EditorWindow.GetWindow(typeof(TypeChangeEditor));
    }
    public static bool IsScriptableSerializable(Type t) {
        return !t.IsAbstract && typeof(ScriptableSerializable).IsAssignableFrom(t);
    }
    public static IEnumerable<object> AllScriptableSerializables() {
        AssetDatabase.ReleaseCachedFileHandles();
        var scriptableSerializableTypes = Assembly.GetAssembly(typeof(ScriptableSerializable)).GetTypes().Where(IsScriptableSerializable);
        foreach (var scriptableSerializableType in scriptableSerializableTypes) {
            foreach (var guid in AssetDatabase.FindAssets($"t:{scriptableSerializableType.Name}")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ForceReserializeAssets(new string[] { path });
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                yield return asset;
                EditorUtility.SetDirty(asset);
            }
        }
    }
    protected void ConvertPrefab(object scriptableSerializable, Action<ISerializable> visitFn = null) {
        TestDetector.AreTestsActive = true;
        try {
            var decodedValueProp = scriptableSerializable.GetType().GetProperty("DecodedValue");
            var value = decodedValueProp.GetValue(scriptableSerializable) as ISerializable;
            if (visitFn != null) {
                visitFn(value);
            }
            decodedValueProp.SetValue(scriptableSerializable, value);
            var versionProp = scriptableSerializable.GetType().GetField("Version");
            versionProp.SetValue(scriptableSerializable, DataVersionController.CURRENT_DATA_VERSION);
        }
        catch (Exception e) {
            errorCount ++;
            Debug.LogWarning($"Encountered error while loading {scriptableSerializable}: \n{e}");
        }
        TestDetector.AreTestsActive = false;
    }
    protected void ReSave() {
        // Convert with no changes = re-save
        errorCount = 0;
        ScriptableSerializable.OnError = () => { errorCount++; };
        StaticResetTool.Reset();
        foreach (var scriptableSerializable in AllScriptableSerializables()) {
            if (dataVersion != DataVersionController.CURRENT_DATA_VERSION) {
                (scriptableSerializable as ScriptableSerializable).Version = dataVersion;
            }
            ConvertPrefab(scriptableSerializable);
        }
        AssetDatabase.SaveAssets();
        if (errorCount == 0) {
            Debug.Log("Successfully re-saved with no errors.");
        }
        else {
            Debug.Log("Some errors encountered.  Assets with errors likely did not change.");
        }
        ScriptableSerializable.OnError = null;
    }
    protected void VisitProperties(ISerializable value) {
        if (
            typeof(SObj).IsAssignableFrom(value.GetType()) ||
            typeof(ScriptableSerializable).IsAssignableFrom(value.GetType())
        ) {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var member in value.GetType().GetProperties(bindingFlags)) {
                // Members that are hidden from inspector should not be serialized.  No secret values please!
                if (member.GetCustomAttribute(typeof(HideInInspector)) != null) {
                    member.SetValue(value, null, bindingFlags, null, null, null);
                    continue;
                }
                if (member.GetCustomAttributes().Any(x => x is Generate)) {
                    // Don't edit in generated fields, they're generated!
                    continue;
                }
                if (!typeof(ISerializable).IsAssignableFrom(member.DeclaringType)) {
                    // This property is from a non-serializable ancestor class
                    continue;
                }
                if (member.GetGetMethod(true)?.GetParameters().Length != 0 ||
                    member.GetSetMethod(true)?.GetParameters().Length != 1) {
                    continue;
                }
                if (!member.CanWrite) {
                    continue;
                }
                var type = member.PropertyType;
                object prop = null;
                try {
                    prop = member.GetValue(value);
                }
                catch (Exception e) {
                    EditorGUILayout.LabelField($"Couldn't load member {member.Name}");
                    if (!GUILayout.Button("Replace")) {
                        throw e;
                    }
                }
                if (object.ReferenceEquals(prop, null)) {
                    addCount++;
                    prop = Generator.Generate(type);
                }
                if (prop is ISerializable serialized) {
                    VisitProperties(serialized);
                }
                member.SetValue(value, prop, bindingFlags, null, null, null);
            }
        }
        else if (typeof(SList<>).IsGenericAncestorOf(value.GetType())) {
            var list = (IList)value;
            var listType = value.GetType().GetTypeAncestor(typeof(SList<>)).GetGenericArguments().Single();
            for (int i = 0; i < list.Count; i++) {
                if (list[i] is ISerializable serialized) {
                    VisitProperties(serialized);
                }
            }
        }
    }
    protected void Traverse() {
        // Visit all properties on serializable objects
        errorCount = 0;
        addCount = 0;
        StaticResetTool.Reset();
        foreach (var scriptableSerializable in AllScriptableSerializables()) {
            ConvertPrefab(scriptableSerializable, VisitProperties);
        }
        AssetDatabase.SaveAssets();
        if (errorCount == 0) {
            Debug.Log($"Successfully traversed with no errors.  Made {addCount} changes");
        }
        else {
            Debug.Log("Some errors encountered.  Assets with errors likely did not change.");
        }
    }
    void OnGUI() {
        EditorGUILayout.LabelField("If you added a field that has a reasonable default, you can use this button to re-save all ScriptableSerializables.");
        EditorGUILayout.LabelField("This is also useful for forcing data migrations.");
        if (GUILayout.Button("Re-Save")) {
            dataVersion = DataVersionController.CURRENT_DATA_VERSION;
            ReSave();
        }

        GUILayout.Space(20f);
        EditorGUILayout.LabelField("Force serializables to repeat migrations from version:");
        var oldEnabled = GUI.enabled;
        // Make sure you turn this back off after you're done
        var ENABLE_DANGEROUS_MIGRATION = false;
        GUI.enabled = ENABLE_DANGEROUS_MIGRATION;
        dataVersion = EditorGUILayout.IntField("Version: ", ENABLE_DANGEROUS_MIGRATION ? dataVersion : DataVersionController.CURRENT_DATA_VERSION);
        GUI.enabled = oldEnabled;
        if (GUILayout.Button("Re-Migrate")) {
            ReSave();
        }

        GUILayout.Space(20f);
        EditorGUILayout.LabelField("Fill out new fields:");
        if (GUILayout.Button("Visit Properties")) {
            Traverse();
        }
    }
}
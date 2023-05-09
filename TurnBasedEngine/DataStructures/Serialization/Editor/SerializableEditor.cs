using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class FoldoutScope : IDisposable {
    public static bool? GlobalFoldout;
    protected static List<object> openFoldouts;
    public static List<object> OpenFoldouts {
        get {
            if (openFoldouts == null) {
                openFoldouts = new List<object>();
            }
            return openFoldouts;
        }
    }
    public bool foldout;
    public FoldoutScope(object editTarget) : this(editTarget, $"{editTarget}") {}
    public FoldoutScope(object editTarget, string foldoutText) {
        var oldFoldout = OpenFoldouts.Any(x => x == editTarget);
        foldout = EditorGUILayout.Foldout(oldFoldout, foldoutText);
        if (oldFoldout != foldout && Event.current.modifiers.HasFlag(EventModifiers.Alt)) {
            GlobalFoldout = foldout;
        }
        if (GlobalFoldout != null) {
            foldout = GlobalFoldout.Value;
        }
        // This prevents negative number entry, and isn't super helpful
        //foldout &= Event.current.keyCode != KeyCode.Minus;
        if (foldout != oldFoldout) {
            if (foldout) {
                OpenFoldouts.Add(editTarget);
            }
            else {
                OpenFoldouts.RemoveAll(x => x == editTarget);
            }
        }
        if (foldout) {
            BeginIndent();
        }
    }

    protected void BeginIndent() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("  "); // indent doesn't work when you use layout
        GUILayout.BeginVertical();
    }

    protected void EndIndent() {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public void Dispose() {
        if (foldout) {
            EndIndent();
        }
    }
}

public abstract class ISerializableEditor<T> where T : ISerializable {
    public abstract bool Draw(T editTarget);
}

[CustomEditor(typeof(ScriptableSerializable))]
public class SerializableEditor : Editor {
    protected static Dictionary<object, string> typeEditStrings;
    public static Dictionary<object, string> TypeEditStrings {
        get {
            if (typeEditStrings == null) {
                typeEditStrings = new Dictionary<object, string>();
            }
            return typeEditStrings;
        }
    }
    protected static Dictionary<Type, object> customEditors;
    protected static Dictionary<Type, object> CustomEditors {
        get {
            if (customEditors == null) {
                customEditors = new Dictionary<Type, object>();
                var editorTypes = typeof(SerializableEditor).Assembly.GetTypes().Where(x =>
                    typeof(ISerializableEditor<>).IsGenericAncestorOf(x) && !x.IsAbstract);
                foreach (var editorType in editorTypes) {
                    var editType = editorType.GetTypeAncestor(typeof(ISerializableEditor<>)).GenericTypeArguments.Single();
                    customEditors[editType] = Constructor.Construct(editorType);
                }
            }
            return customEditors;
        }
    }
    protected static bool manuallyDirty = false;
    protected static SerializedObject serObj;
    protected static object lastEdit;
    public override void OnInspectorGUI() {
        var typedTarget = target as ScriptableSerializable;
        if (!object.ReferenceEquals(typedTarget, lastEdit)) {
            lastEdit = typedTarget;
            serObj = new SerializedObject(target);
        }
        var backingProperty = serObj.FindProperty("EncodedSerializable");
        var versionProperty = serObj.FindProperty("Version");

        serObj.Update();

        manuallyDirty |= Event.current.keyCode == KeyCode.S;
        FoldoutScope.GlobalFoldout = null;

        EditorGUILayout.PropertyField(versionProperty);

        EditorGUI.BeginChangeCheck();
        // We might want a seperate EditorDetector later, but for now, it an just be the "non-gameplay" detector
        TestDetector.AreTestsActive = true;
        try {
            var value = typedTarget.DecodedValue;
            if (object.ReferenceEquals(value, null)) {
                typedTarget.DecodedValue = new SNull();
                manuallyDirty = true;
            }
            if (SNull.IsNull(value) && versionProperty.intValue != DataVersionController.CURRENT_DATA_VERSION) {
                versionProperty.intValue = DataVersionController.CURRENT_DATA_VERSION;
                manuallyDirty = true;
            }
            if (typedTarget.BlockSave) {
                GUILayout.Label("Error reported during load!");
            }
            value = (ISerializable)EditPropertyType(value, typeof(ISerializable));
            value = (ISerializable)EditProperty(value);
            typedTarget.DecodedValue = value;
        }
        catch (Exception e) {
            GUILayout.Label("Error drawing editor.  Has the format changed?");
            GUILayout.Label("Error: " + e.ToString());
            if (GUILayout.Button("Blow it up")) {
                typedTarget.DecodedValue = new SNull();
                typedTarget.BlockSave = false;
                backingProperty.stringValue = typedTarget.DecodedValueAsString; 
                serObj.ApplyModifiedProperties();
            }
            Debug.LogError(e);
            throw e;
        }
        TestDetector.AreTestsActive = false;
        if (EditorGUI.EndChangeCheck() || manuallyDirty || DataVersionController.MadeAnyChanges) {
            backingProperty.stringValue = typedTarget.DecodedValueAsString; 
            DataVersionController.MadeAnyChanges = false;
        }
        manuallyDirty = false;

        serObj.ApplyModifiedProperties();
    }

    protected static void DrawStringProperty(SString s) {
        s.Value = EditorGUILayout.TextField(s);
    }

    protected static void DrawBoolProperty(SBool b) {
        b.Value = EditorGUILayout.Toggle(b);
    }

    protected static void DrawIntProperty(SInt b) {
        b.Value = EditorGUILayout.IntField(b);
    }

    protected static void DrawSEnumProperty(object e) {
        var sEnumType = e.GetType().GetTypeAncestor(typeof(SEnum<>));
        var values = sEnumType.GetProperty("Values").GetMethod.Invoke(null, null);
        var stringValues = (string[])sEnumType.GetProperty("StringValues").GetMethod.Invoke(null, null);
        var valueField = sEnumType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
        var index = (byte)valueField.GetValue(e);
        var newIndex = EditorGUILayout.Popup(index, stringValues);
        if (newIndex != index) {
            valueField.SetValue(e, (byte)newIndex);
        }
    }

    protected static object CreateObject(Type type) {
        object rVal = null;
        if (Constructor.IsRecursivlyConstructableType(type)) {
            // If we have a constructable type, no need to make anything up
            rVal = Constructor.Construct(type);
        }
        else {
            // **Cracks knuckles** otherwise, make something up
            rVal = Generator.Generate(type);
        }
        // We're presumably editing this now, so keep it open
        FoldoutScope.OpenFoldouts.Add(rVal);
        manuallyDirty = true;
        return rVal;
    }

    protected static void DrawCustomEditor(object obj, object editor) {
        var editInterfaceType = editor.GetType().GetTypeAncestor(typeof(ISerializableEditor<>));
        var changed = (bool)editInterfaceType.GetMethod("Draw").Invoke(editor, new object[] { obj });
        manuallyDirty |= changed;
    }

    protected static void DrawListProperty(object l) {
        var list = (IList)l;
        var listType = l.GetType().GetTypeAncestor(typeof(SList<>)).GetGenericArguments().Single();
        using (var horizontal = new EditorGUILayout.HorizontalScope()) {
            EditorGUILayout.LabelField("List of " + listType.Name);
            if (GUILayout.Button("+")) {
                list.Add(CreateObject(listType));
            }
        }
        for (int i = 0; i < list.Count; i++) {
            using (var indentScope = new FoldoutScope(list[i], $"Element {i}")) {
                if (indentScope.foldout) {
                    if (GUILayout.Button("Remove")) {
                        list.RemoveAt(i);
                        break;
                    }
                    list[i] = EditPropertyType(list[i], listType);
                    list[i] = EditProperty(list[i]);
                }
            }
        }
    }

    public static object EditPropertyType(object value, Type propertyType) {
        using (var horiz = new EditorGUILayout.HorizontalScope()) {
            string typeString = "";
            var possibleTypes = Generator.CreateConstructableTypes(propertyType);
            if (possibleTypes.Skip(15).Any()) {
                TypeEditStrings.TryGetValue(value, out typeString);
                typeString = EditorGUILayout.TextField(typeString);
                if (!string.IsNullOrEmpty(typeString)) {
                    possibleTypes = possibleTypes.Where(x => x.ToString().ToLower().Contains(typeString.ToLower()));
                    TypeEditStrings[value] = typeString;
                }
            }
            var possibleTypeArray = possibleTypes.ToArray();
            int currentIndex = possibleTypeArray.IndexOf(x => x == value?.GetType());
            int newIndex = 0;
            if (possibleTypeArray.Skip(1).Any()) {
                newIndex = EditorGUILayout.Popup(currentIndex, possibleTypeArray.Select(x => x.ToString()).ToArray());
            }
            else if (possibleTypeArray.Any()) {
                EditorGUILayout.LabelField(possibleTypeArray[0].ToString());
            }
            else {
                EditorGUILayout.LabelField("No types possible");
            }
            if (newIndex != currentIndex && possibleTypeArray.Any()) {
                // If there is only one possible type and this isn't it.  Non-empty type string means this type may be possible, just filtered out
                if (!SNull.IsNull(value) && (newIndex == 0 && possibleTypeArray.Count() == 1) && string.IsNullOrEmpty(typeString)) {
                    if (GUILayout.Button("Delete impossible value?")) {
                        manuallyDirty = true;
                        return null;
                    }
                    return value;
                }
                value = CreateObject(possibleTypeArray[newIndex]);
            }
            if (GUILayout.Button("Copy")) {
                GUIUtility.systemCopyBuffer = System.Convert.ToBase64String((value as ISerializable).ToStream().data);
            }
            if (GUILayout.Button("Paste")) {
                // Copy-paste across versions won't do migrations
                var newValue = new Stream(System.Convert.FromBase64String(GUIUtility.systemCopyBuffer), DataVersionController.CURRENT_DATA_VERSION).Consume();
                if (newValue == null) {
                    Debug.LogError("There's no object in your clipboard");
                }
                else if (!propertyType.IsAssignableFrom(newValue.GetType())) {
                    Debug.LogError($"Can't assign type of {propertyType.Name} from value in clipboard.  Clipboard object is a {newValue.GetType().Name}.");
                }
                else {
                    FoldoutScope.OpenFoldouts.Add(newValue);
                    return newValue;
                }
            }
        }
        return value;
    }

    protected static void DrawObjectProperty(object value) {
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        using (var verticalLayout = new EditorGUILayout.VerticalScope()) {
            foreach (var member in value.GetType().GetProperties(bindingFlags)) {
                if (member.GetGetMethod(true)?.GetParameters().Length != 0 ||
                    member.GetSetMethod(true)?.GetParameters().Length != 1) {
                    continue;
                }
                if (!member.CanWrite) {
                    continue;
                }
                if (!typeof(ISerializable).IsAssignableFrom(member.DeclaringType)) {
                    // This property is from a non-serializable ancestor class
                    continue;
                }
                if (!typeof(ISerializable).IsAssignableFrom(member.PropertyType)) {
                    // We can't edit this
                    continue;
                }
                // If we do this with the extension, it won't search the hierarchy!!!
                var attributes = Attribute.GetCustomAttributes(member, true);
                if (attributes.OfType<HideInInspector>().Any()) {
                    var foundBadData = true;
                    try {
                        foundBadData = !SNull.IsNull(member.GetValue(value));
                    }
                    // If there was an error, we found bad data!
                    catch { }
                    if (foundBadData) {
                        member.SetValue(value, null);
                        manuallyDirty = true;
                        Debug.LogWarning($"Found bad serialized data in property: {member.Name}.  Deleting...");
                    }
                    continue;
                }
                if (member.GetCustomAttributes().Any(x => x is Generate || x is HideInInspector)) {
                    // Don't edit in generated fields, they're generated!
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
                    prop = Generator.Generate(type);
                    manuallyDirty = true;
                }
                using (var indentScope = new FoldoutScope(prop, member.Name)) {
                    if (indentScope.foldout) {
                        prop = EditPropertyType(prop, member.PropertyType);
                        prop = EditProperty(prop);
                    }
                }
                member.SetValue(value, prop, bindingFlags, null, null, null);
            }
        }
    }

    public static object EditProperty(object value) {
        try {
            if (SNull.IsNull(value)) {
                // Draw null prop?
            }
            else if (CustomEditors.TryGetValue(value.GetType(), out var editor)) {
                DrawCustomEditor(value, editor);
            }
            else if (value is SString) {
                DrawStringProperty(value as SString);
            }
            else if (value is SBool) {
                DrawBoolProperty(value as SBool);
            }
            else if (value is SInt) {
                DrawIntProperty(value as SInt);
            }
            else if (typeof(SEnum<>).IsGenericAncestorOf(value.GetType())) {
                DrawSEnumProperty(value);
            }
            else if (
                typeof(SObj).IsAssignableFrom(value.GetType()) ||
                typeof(ScriptableSerializable).IsAssignableFrom(value.GetType())
            ) {
                DrawObjectProperty(value);
            }
            else if (typeof(SList<>).IsGenericAncestorOf(value.GetType())) {
                DrawListProperty(value);
            }
        }
        catch (Exception e) {
            GUILayout.Label("Error drawing property");
            Debug.LogWarning("Error: " + e.ToString());
            if (GUILayout.Button("Remove property")) {
                return Constructor.Construct<SNull>();
            }
        }
        return value;
    }
}
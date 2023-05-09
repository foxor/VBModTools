using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(ScriptableDatabase<T>))]
public abstract class ScriptableDatabaseRefreshEditor<T> : Editor where T : ScriptableObject
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Database"))
        {
            var dbProp = serializedObject.FindProperty("Database");
            dbProp.ClearArray();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            dbProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                dbProp.GetArrayElementAtIndex(i).objectReferenceValue = asset;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

public abstract class SerializableDatabaseRefreshEditor<T> : Editor where T : ISerializable
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Database"))
        {
            var found = new List<ScriptableSerializable>();
            var dbProp = serializedObject.FindProperty("Database");
            dbProp.ClearArray();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(ScriptableSerializable)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableSerializable>(assetPath);
                if (asset.DecodedValue is T)
                {
                    found.Add(asset);
                }
            }
            dbProp.arraySize = found.Count;
            for (int i = 0; i < found.Count; i++)
            {
                dbProp.GetArrayElementAtIndex(i).objectReferenceValue = found[i];
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
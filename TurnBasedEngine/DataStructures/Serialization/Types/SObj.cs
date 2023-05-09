using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;

[TypeIndex(2)]
public class SObj : SList<ISerializable>, IPoolable {
    public bool HasProperty(int key) {
        return innerList.Count > key && !SNull.IsNull(innerList[key]);
    }

    public override string ToString() {
        var activeStack = Constructor.Construct<Stack<SObj>>();
        var keysStack = Constructor.Construct<Stack<int[]>>();
        var indexStack = Constructor.Construct<Stack<int>>();
        try {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetType().GetNameCached());
            builder.Append("{");

            activeStack.Push(this);
            keysStack.Push(Enumerable.Range(0, innerList.Count).ToArray());
            indexStack.Push(0);

            while (activeStack.Any()) {
                var active = activeStack.Pop();
                var keys = keysStack.Pop();
                bool finishedAllKeys = true;
                for (var index = indexStack.Pop(); index < keys.Length; index++) {
                    if (builder.Length > 150) {
                        // This could constitute a warning, but there might be valid use cases
                        return builder.ToString();
                    }
                    int key = keys[index];
                    object value = active.innerList[key];
                    if (value is SObj) {
                        builder.Append(value.GetType().GetNameCached());
                        builder.Append("{");
                        activeStack.Push(active);
                        keysStack.Push(keys);
                        // we'll be done serializing this element when we come back
                        indexStack.Push(index + 1);

                        activeStack.Push(value as SObj);
                        keysStack.Push(Enumerable.Range(0, (value as SObj).innerList.Count).ToArray());
                        indexStack.Push(0);
                        finishedAllKeys = false;
                        break;
                    }
                    else if (value == null) {
                        builder.Append("null");
                    }
                    else {
                        builder.Append(value.ToString());
                    }
                    if (index + 1 < keys.Length) {
                        builder.Append(",");
                    }
                }
                if (finishedAllKeys) {
                    if (builder.Length > 1 && builder[builder.Length - 1] == ',' && builder[builder.Length - 2] == '}') {
                        // If the string currently ends with "},", we want it to now end with "}},"
                        // Is there a better way to do this?  Of course!  Do I care?  No!
                        builder.Remove(builder.Length - 1, 1);
                    }
                    builder.Append("}");
                    builder.Append(",");
                }
            }
            if (builder.Length > 0 && builder[builder.Length - 1] == ',') {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }
        catch {
            return "<tostring exception>";
        }
        finally {
            indexStack.Clear();
            keysStack.Clear();
            activeStack.Clear();
            Pool.Return(indexStack);
            Pool.Return(keysStack);
            Pool.Return(activeStack);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions {
	public static void DestroyAllChildren(this Transform badParent) {
		for (int i = badParent.childCount - 1; i >= 0; i--) {
			GameObject.Destroy(badParent.GetChild(i).gameObject);
		}
	}
}
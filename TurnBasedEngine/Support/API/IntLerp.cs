using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntLerp {
	public static int Lerp(int a, int b, float t) {
		return Mathf.RoundToInt(Mathf.Lerp(a * 1f, b * 1f, t));
	}
}
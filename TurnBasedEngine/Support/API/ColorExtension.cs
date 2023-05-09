using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorLerp {
	public static Color Lerp(Color a, Color b, float interpolation) {
		return new Color(
			Mathf.Lerp(a.r, b.r, interpolation),
			Mathf.Lerp(a.g, b.g, interpolation),
			Mathf.Lerp(a.b, b.b, interpolation),
			Mathf.Lerp(a.a, b.a, interpolation)
		);
	}
}
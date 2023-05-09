using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween {
    public static bool IsSimulating { private get; set; }

	public static IEnumerator BasicTween (float seconds, Action<float> callback) {
		float elapsedTime = 0f;
		while(elapsedTime < seconds && !IsSimulating) {
			callback(elapsedTime / seconds);
			yield return null;
			elapsedTime += Time.deltaTime;
		}
		callback(1f);
	}

	public static Action<float> Invert (Action<float> fn) {
		return (x) => fn(1f - x);
	}

	public static IEnumerator TweenGeneric<T> (float seconds, T startValue, T endValue, Action<T> callback, Func<T, T, float, T> converter) {
		yield return BasicTween(seconds, x => callback(converter(startValue, endValue, x)));
	}

	public static IEnumerator FloatTween (float seconds, float startValue, float endValue, Action<float> callback) {
		return TweenGeneric<float>(seconds, startValue, endValue, callback, Mathf.Lerp);
	}

	public static IEnumerator IntTween (float seconds, int startValue, int endValue, Action<int> callback) {
		return TweenGeneric<int>(seconds, startValue, endValue, callback, IntLerp.Lerp);
	}

	public static IEnumerator Vector3Tween (float seconds, Vector3 startValue, Vector3 endValue, Action<Vector3> callback) {
		return TweenGeneric<Vector3>(seconds, startValue, endValue, callback, Vector3.Lerp);
	}

	public static IEnumerator RectTween (float seconds, Rect startValue, Rect endValue, Action<Rect> callback) {
        throw new NotImplementedException();
		//return TweenGeneric<Rect>(seconds, startValue, endValue, callback, RectTweenClass.Tween);
	}

	public static IEnumerator ColorTween (float seconds, Color startValue, Color endValue, Action<Color> callback) {
		return TweenGeneric<Color>(seconds, startValue, endValue, callback, ColorLerp.Lerp);
	}

	public static IEnumerator Wait (float seconds) {
		yield return new WaitForSeconds(seconds);
	}
}
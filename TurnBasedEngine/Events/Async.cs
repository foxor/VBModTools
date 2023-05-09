using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Async : MonoBehaviour {
	public static Coroutine InstantReturn;

	protected static Async _coroutineRunner;
	protected static Async CoroutineRunner {
		get {
			if (!Application.isPlaying) {
				throw new System.ApplicationException("Don't run the game in edit mode!");
			}
			if (_coroutineRunner == null) {
				var gameObject = new GameObject();
				gameObject.name = "Coroutine Runner";
				GameObject.DontDestroyOnLoad(gameObject);
				_coroutineRunner = gameObject.AddComponent<Async>();
			}
			return _coroutineRunner;
		}
	}

	public void Start() {
		InstantReturn = StartCoroutine(WaitOneFrame());
	}

	private IEnumerator WaitOneFrame() {
		yield return null;
	}

	public static Coroutine StartAsync(IEnumerator newCoroutine) {
		return CoroutineRunner.StartCoroutine(newCoroutine);
	}
}
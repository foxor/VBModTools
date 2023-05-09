using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediatedTypedEvent<T> where T : class {
	public class Registration {
		public Func <T, bool> Qualifier;
		public Action <T> Fire;
	}
	private List<Registration> listeners = new List<Registration>();

	public void Invoke(T context = null) {
		foreach (var registration in listeners) {
			if (registration.Qualifier(context)) {
				registration.Fire(context);
			}
		}
	}

	public void Register(Action<T> toRegister, T context = null) {
		listeners.Add(new Registration() {
			Qualifier = (x) => x == context,
			Fire = toRegister,
		});
	}
	public void Deregister(Action<T> toRegister, T context = null) {
		listeners.RemoveAll(x => x.Qualifier(context) && x.Fire == toRegister);
	}

	public void Register(Registration listener) {
		listeners.Add(listener);
	}

	public void Deregister(Registration listener) {
		listeners.Remove(listener);
	}
}
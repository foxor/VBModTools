using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PerformanceTimer  : IDisposable
{
	private Stopwatch stopwatch = new Stopwatch();
	private Action<float> onComplete;

	public PerformanceTimer(Action<float> onComplete) {
		this.onComplete = onComplete;
		stopwatch.Start ();
	}

	public void Dispose() {
		stopwatch.Stop();
		onComplete ((float)stopwatch.Elapsed.TotalSeconds);
	}
}
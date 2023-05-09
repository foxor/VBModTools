using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BivariateNormal
{
	public static readonly float STD_FRACTION = 0.2f;

	public static float MonoSample(float mean)
    {
		return Sample (mean, mean * STD_FRACTION);
	}

	public static float Sample(float mean, float deviation)
    {
		float U1 = Random.Range (0f, 1f);
		float U2 = Random.Range (0f, 360f);
		return Mathf.Sqrt (-2f * Mathf.Log (U1)) * Mathf.Cos (U2) * deviation + mean;
	}

	public static void Test()
    {
		const float TARGET_MEAN = 531f;
		const float TARGET_STD = 8.31f;
		const int SAMPLES = 10000;

		float[] samples = new float[SAMPLES];
		for (int i = 0; i < SAMPLES; i++)
        {
			samples [i] = Sample (TARGET_MEAN, TARGET_STD);
		}

		float mean = 0f;
		for (int i = 0; i < SAMPLES; i++)
        {
			mean += samples [i] / SAMPLES;
		}

		float sqr_variance = 0f;
		for (int i = 0; i < SAMPLES; i++)
        {
			float delta = samples [i] - mean;
			sqr_variance += (delta * delta) / SAMPLES;
		}
		float std = Mathf.Sqrt (sqr_variance);

		Debug.Log ("Target mean: " + TARGET_MEAN + ", Calculated mean: " + mean + ", Error: " + (mean - TARGET_MEAN));
        Debug.Log ("Target std: " + TARGET_STD + ", Calculated std: " + std + ", Error: " + (std - TARGET_STD));
	}
}
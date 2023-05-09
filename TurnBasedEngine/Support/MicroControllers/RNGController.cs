using System.Runtime.InteropServices.ComTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[SerializableEnum]
[TypeIndex(39)]
public enum RNG : byte {
    Pathing,
    Skill,
    AI,
    Metagame,
    Cloud,
    Generation,
    Stat,
    View,
    _COUNT
}

public static class RNGController {
    public static ConsistentRandom[] sources;
    public static ConsistentRandom[] Sources {
        get {
            if (sources == null) {
                Initialize();
            }
            return sources;
        }
    }

    internal static int Seed { get => TestDetector.AreTestsActive ? 12345 : SaveState.Singleton.CurrentRun.Seed; }
    internal static SList<SInt> Rerolls { get => TestDetector.AreTestsActive ? null : SaveState.Singleton.CurrentRun.Rerolls; }

    public static void Initialize() {
        var master = new ConsistentRandom(Seed);
        var rerolls = Rerolls;
        var count = (byte)RNG._COUNT;
        sources = new ConsistentRandom[count];
        for (var i = 0; i < count; i++) {
            sources[i] = new ConsistentRandom(master.Next());
            if (rerolls != null) {
                for (var r = 0; r < rerolls[i]; r++) {
                    sources[i].Sample();
                }
            }
        }
    }
    private static ConsistentRandom FetchForSample(RNG rng) {
        var source = Sources[(byte)rng];
        if (!TestDetector.AreTestsActive) {
            SaveState.Singleton.CurrentRun.Rerolls[(byte)rng] += 1;
        }
        return source;
    }
    private static void CountRoll(RNG rng) {
        if (!TestDetector.AreTestsActive) {
            SaveState.Singleton.CurrentRun.Rerolls[(byte)rng]++;
        }
    }
    public static T Choose<T>(this RNG rng, IEnumerable<T> options) {
        if (options == null) {
            return default(T);
        }
        var count = options.Count();
        // It's not great if we roll dice for no reason.
        if (count == 0) {
            return default(T);
        }
        if (count == 1) {
            return options.First();
        }
        var source = FetchForSample(rng);
        var toSkip = source.Next(count);
        return options.Skip(toSkip).First();
    }
    public static IEnumerable<T> Choose<T>(this RNG rng, IEnumerable<T> options, int count) {
        return rng.Shuffle(options).Take(count);
    }
    public static T Choose<T>(this RNG rng) where T : System.Enum {
        return rng.Choose<T>((T[])Enum.GetValues(typeof(T)));
    }
    public static IEnumerable<T> Choose<T>(this RNG rng, int count) where T : System.Enum {
        return rng.Choose<T>((T[])Enum.GetValues(typeof(T)), count);
    }
    public static T ChooseOrDefault<T>(this RNG rng, IEnumerable<T> options) {
        var source = FetchForSample(rng);
        CountRoll(rng);
        var toSkip = source.Next(options.Count());
        return options.Skip(toSkip).FirstOrDefault();
    }
    public static bool Chance(this RNG rng, float chance) {
        var source = FetchForSample(rng);
        CountRoll(rng);
        return source.Sample() <= chance;
    }
    public static IEnumerable<T> Shuffle<T>(this RNG rng, IEnumerable<T> deck) {
        var source = Sources[(byte)rng];
        // The ToArray is to avoid re-shuffling
        return deck.OrderBy(x => {
            CountRoll(rng);
            return source.Next();
        }).ToArray();
    }
    // Returns [1, sides]
    public static int Roll(this RNG rng, int sides) {
        var source = FetchForSample(rng);
        CountRoll(rng);
        return source.Next(sides - 1) + 1;
    }
    // Inclusive
    public static int Between(this RNG rng, int min, int max) {
        var source = FetchForSample(rng);
        CountRoll(rng);
        return source.Next(max - min + 1) + min;
    }
    public static float Between(this RNG rng, float min, float max) {
        return (rng.Sample() * (max - min)) + min;
    }
    public static float Sample(this RNG rng) {
        var source = FetchForSample(rng);
        CountRoll(rng);
        return source.Sample();
    }
    public static float Sample(this RNG rng, float min, float max) {
        var source = FetchForSample(rng);
        CountRoll(rng);
        return (source.Sample() * (max - min)) + min;
    }
}
using System;

// The .net spec doesn't guarantee the algorithm will always be the same, so
// this is a snapshot that should always function the same.
// Originally from: https://stackoverflow.com/questions/42823244/will-system-random-always-generate-predictable-numbers-for-a-given-seed-across-p
public class ConsistentRandom {
    private const int MBIG = Int32.MaxValue;
    private const int MSEED = 161803398;
    private const int MZ = 0;

    private int inext;
    private int inextp;
    private int[] SeedArray = new int[56];

    public ConsistentRandom(int seed) {
        int ii;
        int mj, mk;

        int subtraction = (seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(seed);
        mj = MSEED - subtraction;
        SeedArray[55] = mj;
        mk = 1;
        for (int i = 1; i < 55; i++)
        {
            ii = (21 * i) % 55;
            SeedArray[ii] = mk;
            mk = mj - mk;
            if (mk < 0) mk += MBIG;
            mj = SeedArray[ii];
        }
        for (int k = 1; k < 5; k++)
        {
            for (int i = 1; i < 56; i++)
            {
                SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                if (SeedArray[i] < 0) SeedArray[i] += MBIG;
            }
        }
        inext = 0;
        inextp = 21;
    }
    public float Sample() {
        return (InternalSample() * (1f / MBIG));
    }

    private int InternalSample() {
        int retVal;
        int locINext = inext;
        int locINextp = inextp;

        if (++locINext >= 56) locINext = 1;
        if (++locINextp >= 56) locINextp = 1;

        retVal = SeedArray[locINext] - SeedArray[locINextp];

        if (retVal == MBIG) retVal--;
        if (retVal < 0) retVal += MBIG;

        SeedArray[locINext] = retVal;

        inext = locINext;
        inextp = locINextp;

        return retVal;
    }

    public int Next() {
        return InternalSample();
    }

    // Returns [0, max)
    public int Next(int max) {
        return (int)(Sample() * max);
    }
}
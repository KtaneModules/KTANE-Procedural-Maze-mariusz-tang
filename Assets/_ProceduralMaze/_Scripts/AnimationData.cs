using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class AnimationData {
    public static int[] StrikeX => new int[] { 0, 5, 7, 10, 14, 15, 20, 21, 25, 28, 30, 35 };

    public static int[][] Layers => new int[][] {
        new int[] { 30, 31, 32, 33, 34, 35 },
        new int[] { 24, 25, 26, 27, 28, 29 },
        new int[] { 18, 19, 20, 21, 22, 23 },
        new int[] { 12, 13, 14, 15, 16, 17 },
        new int[] { 6, 7, 8, 9, 10, 11 },
        new int[] { 0, 1, 2, 3, 4, 5 },
        new int[] { 0, 6, 12, 18, 24, 30 },
        new int[] { 1, 7, 13, 19, 25, 31 },
        new int[] { 2, 8, 14, 20, 26, 32 },
        new int[] { 3, 9, 15, 21, 27, 33 },
        new int[] { 4, 10, 16, 22, 28, 34 },
        new int[] { 5, 11, 17, 23, 29, 35 },
        new int[] { 0, 1, 2, 3, 4, 5 },
        new int[] { 6, 7, 8, 9, 10, 11 },
        new int[] { 12, 13, 14, 15, 16, 17 },
        new int[] { 18, 19, 20, 21, 22, 23 },
        new int[] { 24, 25, 26, 27, 28, 29 },
        new int[] { 30, 31, 32, 33, 34, 35 },
        new int[] { 5, 11, 17, 23, 29, 35 },
        new int[] { 4, 10, 16, 22, 28, 34 },
        new int[] { 3, 9, 15, 21, 27, 33 },
        new int[] { 2, 8, 14, 20, 26, 32 },
        new int[] { 1, 7, 13, 19, 25, 31 },
        new int[] { 0, 6, 12, 18, 24, 30 },
    };

    public static int[][] SpiralOut => new int[][] {
        new int[] { 14, 15, 20, 21 },
        new int[] { 8, 16, 19, 27 },
        new int[] { 9, 13, 22, 26, },
        new int[] { 3, 4, 6, 12, 23, 29, 31, 32 },
        new int[] { 0, 5, 7, 10, 25, 28, 30, 35 },
        new int[] { 1, 2, 11, 17, 18, 24, 33, 34 },
    };

    public static int[] RandomSingle => Enumerable.Range(0, 36).OrderBy(i => UnityEngine.Random.Range(0, 1f)).ToArray();
}
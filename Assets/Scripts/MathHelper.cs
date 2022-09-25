using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static float RandomSign()
    {
        float value = Random.Range(0, 2);

        return value == 0 ? -1 : 1;
    }

    public static void ShuffleArray(ref int[] array)
    {
        System.Random rand = new System.Random();

        int count = array.Length;

        for (int i = array.Length-1; i > 1; i--)
        {
            int rnd = rand.Next(i+1);

            int value = array[rnd];
            array[rnd] = array[i];
            array[i] = value;
        }
    }
}

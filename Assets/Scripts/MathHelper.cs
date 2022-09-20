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
}

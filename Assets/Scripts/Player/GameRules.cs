using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules
{
    public static byte playerAmount = 2;

    public static byte wormsPerPlayer = 3;

    public static float wormsMaxHealth = 50;

    public const float maxSpeed = 4;
    public const float jumpHeight = 3.5f;

    public const float maxDistance = 5;

    public static Color32[] playerColors = new Color32[]
    {
        Color.blue, Color.red, Color.green, Color.yellow
    };
}

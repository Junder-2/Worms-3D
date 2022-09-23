using Unity.Mathematics;
using UnityEngine;

public class GameRules
{
    public static byte playerAmount = 2;

    public static byte wormsPerPlayer = 3;

    public static half wormsMaxHealth = (half)50;

    public static half roundTimer = (half)30;

    public const float maxSpeed = 4;
    public const float jumpHeight = 3.5f;

    public const float maxDistance = 5;

    public const byte MinPlayers = 2;
    public const byte MaxPlayers = 4;

    public const byte MinWorms = 1;
    public const byte MaxWorms = 4;

    public const byte MinHealth = 10;
    public const byte MaxHealth = 50;

    public const byte MinRoundTime = 5;
    public const byte MaxRoundTime = 40;

    public static Color32[] playerColors = {
        Color.blue, Color.red, Color.green, Color.yellow
    };
}

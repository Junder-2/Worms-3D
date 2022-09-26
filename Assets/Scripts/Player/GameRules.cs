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

    public static Color32[] playerUIColors = {
        Color.blue, Color.red, Color.green, Color.yellow
    };

    public static Color32[] playerPresetColors =
    {
        new Color32(0xD9,0x83,0x6B, 0xFF), new Color32(0xE0,0x77,0x80, 0xFF), 
        new Color32(0xC8,0x5B,0x3D, 0xFF), new Color32(0xB7, 0x98, 0x79, 0xFF)
    };
}

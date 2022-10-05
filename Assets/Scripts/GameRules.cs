using Unity.Mathematics;
using UnityEngine;

public class GameRules
{
    public static byte PlayerAmount = 2;

    public static byte WormsPerPlayer = 3;

    public static half WormsMaxHealth = (half)50;

    public static half RoundTimer = (half)30;

    public const float MaxSpeed = 4;
    public const float JumpHeight = 3.5f;

    public const byte MinPlayers = 2;
    public const byte MaxPlayers = 4;

    public const byte MinWorms = 1;
    public const byte MaxWorms = 4;

    public const byte MinHealth = 10;
    public const byte MaxHealth = 50;

    public const byte MinRoundTime = 5;
    public const byte MaxRoundTime = 40;

    public static int[] PlayerAssignedPreset =
    {
        0, 1, 2, 4
    };

    public static Color32[] PlayerUIColors = {
        Color.blue, Color.red, Color.green, Color.yellow
    };

    public static Color32[] PlayerPresetColors =
    {
        new Color32(0xD9,0x83,0x6B, 0xFF), new Color32(0xE0,0x77,0x80, 0xFF), 
        new Color32(0xC8,0x5B,0x3D, 0xFF), new Color32(0xB7, 0x98, 0x79, 0xFF)
    };

    public static float[] EyeLidDefault =
    {
        .55f, 1f, .7f, .6f
    };

    public static float[] EyeLidTilt =
    {
        -10f, 0f, 4f, 20f
    };
}

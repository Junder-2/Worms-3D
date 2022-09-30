using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    public struct PlayerData
    {
        public int[] weaponAmount;

        public byte[] worms;
    }
    
    public struct WormState
    {
        public Transform Transform;
        
        public float camYaw, camPitch, camZoom;

        public Transform camFollow;

        public Vector3 camPos, camRot;

        public float maxMoveSpeed;
        public float jumpHeight;

        public float health;
        public bool alive;

        public byte wormIndex, playerIndex;

        public Vector3 velocity;

        public bool freezeCamPitch;

        public bool freezeCamYaw;

        public byte currentWeapon;

        public float currentWaterLevel;

        public bool currentPlayer;
    }

    public const float SlopeLimit = .65f;
    
    public const float hitboxHeight = .3f;
    
    public const float TerminalVel = -20;
}

using UnityEngine;

namespace Player
{
    public static class PlayerInfo
    {
        public struct PlayerData
        {
            public int[] WeaponAmount;

            public int[] Worms;
        }
    
        public struct WormState
        {
            public Transform Transform;
        
            public float CamYaw, CamPitch, CamZoom;

            public Transform CamFollow;

            public Vector3 CamPos, CamRot;

            public float MaxMoveSpeed;
            public float JumpHeight;

            public float Health;
            public bool Alive;

            public byte WormIndex, PlayerIndex;

            public Vector3 Velocity;

            public bool FreezeCamPitch;

            public bool FreezeCamYaw;

            public byte CurrentWeapon;

            public float CurrentWaterLevel;

            public bool CurrentPlayer;
        }

        public const float SlopeLimit = .65f;
    
        public const float HitboxHeight = .3f;
    
        public const float TerminalVel = -20;
    }
}

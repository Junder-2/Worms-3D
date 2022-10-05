using System;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance;
    
        [SerializeField] private Vector2 turnSpeed = new Vector2(45, 45);

        [SerializeField] private float camOffset = -8;

        private Vector3 _virtualPos;
    
        private static LayerMask _camCollision => LayerMask.GetMask("Default");

        private void Awake()
        {
            Instance = this;
        }

        public void InstantiateWormCam(ref WormController wormController)
        {
            ref var playerState = ref wormController.State;
            ref var yaw = ref playerState.CamYaw;
            ref var pitch = ref playerState.CamPitch;

            yaw = Random.Range(0f, 360f);
            pitch = Random.Range(-30f, -15f);
            playerState.CamZoom = 1;

            playerState.CamRot = new Vector3(-pitch, yaw, 0);

            Vector3 camPos = new Vector3(
                Mathf.Sin(yaw * Mathf.Deg2Rad) * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad),
                Mathf.Sin(pitch * Mathf.Deg2Rad) * camOffset,
                Mathf.Cos(yaw * Mathf.Deg2Rad) * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad));

            playerState.CamPos = camPos;
        }

        private const float FloorOffset = .5f;
        private const float WallOffset = .2f;

        public void UpdateCamera(ref PlayerInfo.WormState wormState, ref PlayerInput.InputAction inputs)
        {
            float deltaTime = Time.deltaTime;//inputs.deltaTime;

            //ref var inputs = ref playerState.input;
            float yaw = wormState.CamYaw;
            float pitch = wormState.CamPitch;
            if(!wormState.FreezeCamYaw)
                yaw += inputs.cameraInput.x * deltaTime * turnSpeed.x;
            if(!wormState.FreezeCamPitch)
                pitch -= inputs.cameraInput.y * deltaTime * turnSpeed.y;

            float offset = camOffset * wormState.CamZoom;

            yaw %= 360;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            float zoomMulti = 1;

            float sinYaw = Mathf.Sin(yaw * Mathf.Deg2Rad);
            float cosYaw = Mathf.Cos(yaw * Mathf.Deg2Rad);

            Vector3 camPos = new Vector3(
                sinYaw * offset * Mathf.Cos(pitch * Mathf.Deg2Rad),
                Mathf.Sin(pitch * Mathf.Deg2Rad) * offset,
                cosYaw * offset * Mathf.Cos(pitch * Mathf.Deg2Rad));
        
            Vector3 newPos = wormState.Transform.position + camPos;
        
            Vector3 dir = Vector3.Cross(camPos.normalized, Vector3.up) * -Mathf.Sign(inputs.cameraInput.x);
        
            RaycastHit hit;

            if (Physics.Raycast(newPos, dir, out hit, 5f, _camCollision))
            {
                if (hit.distance < WallOffset)
                {
                    yaw -= inputs.cameraInput.x * deltaTime * turnSpeed.x;
                }
            }
        
            sinYaw = Mathf.Sin(yaw * Mathf.Deg2Rad);
            cosYaw = Mathf.Cos(yaw * Mathf.Deg2Rad);
        
            camPos = new Vector3(
                sinYaw * offset * Mathf.Cos(pitch * Mathf.Deg2Rad),
                camPos.y,
                cosYaw * offset * Mathf.Cos(pitch * Mathf.Deg2Rad));

            _virtualPos = wormState.CamFollow.position + camPos;

            if (Physics.Raycast(transform.position + .5f * Vector3.up, Vector3.down, out hit, 10f, _camCollision))
            {
                float dist = _virtualPos.y - hit.point.y;
            
                //print(dist);

                if (dist < FloorOffset)
                {
                    zoomMulti = Mathf.Clamp(1 - Mathf.Abs(dist), .2f, 1);

                    camPos.y *= zoomMulti;
                    //camPos.y += floorOffset;

                    if (zoomMulti <= .2f)
                    {
                        pitch += inputs.cameraInput.y * deltaTime * turnSpeed.y;
                    }
                }   
            }

            camPos.y += FloorOffset;
            camPos.x *= zoomMulti;
            camPos.z *= zoomMulti;

            newPos = wormState.CamFollow.position + camPos;

            transform.eulerAngles = new Vector3(-pitch, yaw, 0);
            transform.position = newPos;

            wormState.CamYaw = yaw;
            wormState.CamPitch = pitch;
            wormState.CamPos = camPos;
            wormState.CamRot = transform.eulerAngles;

            inputs.camYaw = yaw;
        }

        [SerializeField] private float transitionSpeed = 5;

        public bool TransitionCamera(Vector3 startCamPos, Vector3 targetCamPos, Vector3 startCamRot, Vector3 targetCamRot, float delta)
        {        
            var position = transform.position;
        
            float distance = Mathf.Max(Vector3.Distance(startCamPos, targetCamPos), Single.Epsilon);

            float t = 1 - Mathf.Clamp01(Vector3.Distance(position, targetCamPos)/distance);

            position = Vector3.MoveTowards(position, targetCamPos, transitionSpeed * delta);
            transform.position = position;
            transform.eulerAngles = Vector3.Slerp(startCamRot, targetCamRot, t);

            return t >= 1;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    
    [SerializeField] private Vector2 turnSpeed = new Vector2(45, 45);

    [SerializeField] private float camOffset = -8;

    private Vector3 _virtualPos;
    
    private LayerMask _camCollision => LayerMask.GetMask("Default");

    private void Awake()
    {
        Instance = this;
    }

    public void InstantiateWormCam(ref WormController wormController)
    {
        ref var playerState = ref wormController.State;
        ref var yaw = ref playerState.camYaw;
        ref var pitch = ref playerState.camPitch;

        yaw = Random.Range(0f, 360f);
        pitch = Random.Range(-30f, -15f);

        playerState.camRot = new Vector3(-pitch, yaw, 0);

        Vector3 camPos = new Vector3(
            Mathf.Sin(yaw * Mathf.Deg2Rad) * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad),
            Mathf.Sin(pitch * Mathf.Deg2Rad) * camOffset,
            Mathf.Cos(yaw * Mathf.Deg2Rad) * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad));

        playerState.camPos = wormController.gameObject.transform.position + camPos;
    }

    private const float floorOffset = .5f;
    private const float wallOffset = .2f;

    public void UpdateCamera(ref WormController.PlayerState playerState, ref PlayerInput.InputAction inputs)
    {
        float deltaTime = Time.deltaTime;//inputs.deltaTime;

        //ref var inputs = ref playerState.input;
        float yaw = playerState.camYaw;
        float pitch = playerState.camPitch;
        if(!playerState.freezeCamYaw)
            yaw += inputs.cameraInput.x * deltaTime * turnSpeed.x;
        if(!playerState.freezeCamPitch)
            pitch -= inputs.cameraInput.y * deltaTime * turnSpeed.y;

        yaw %= 360;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        float zoomMulti = 1;

        float sinYaw = Mathf.Sin(yaw * Mathf.Deg2Rad);
        float cosYaw = Mathf.Cos(yaw * Mathf.Deg2Rad);

        Vector3 camPos = new Vector3(
            sinYaw * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad),
            Mathf.Sin(pitch * Mathf.Deg2Rad) * camOffset,
            cosYaw * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad));

        RaycastHit hit;
        
        _virtualPos = playerState.Transform.position + camPos;

        if (Physics.Raycast(transform.position + .5f * Vector3.up, Vector3.down, out hit, 10f, _camCollision))
        {
            float dist = _virtualPos.y - hit.point.y;
            
            //print(dist);

            if (dist < floorOffset)
            {
                zoomMulti = Mathf.Clamp(1 - Mathf.Abs(dist), .2f, 1);

                camPos.y *= zoomMulti;
                camPos.y += floorOffset;

                if (zoomMulti <= .2f)
                {
                    pitch += inputs.cameraInput.y * deltaTime * turnSpeed.y;
                }
            }   
        }

        Vector3 newPos = playerState.Transform.position + camPos;

        Vector3 dir = new Vector3(sinYaw * -Mathf.Sign(inputs.cameraInput.x), 0,
            cosYaw * -Mathf.Sign(inputs.cameraInput.x)).normalized;

        if (Physics.Raycast(newPos, dir, out hit, 5f, _camCollision))
        {
            if (hit.distance < wallOffset)
            {
                yaw -= inputs.cameraInput.x * deltaTime * turnSpeed.x;
            }
        }
        
        sinYaw = Mathf.Sin(yaw * Mathf.Deg2Rad);
        cosYaw = Mathf.Cos(yaw * Mathf.Deg2Rad);

        camPos = new Vector3(
            sinYaw * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad)*zoomMulti,
            camPos.y,
            cosYaw * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad)*zoomMulti);
        
        newPos = playerState.Transform.position + camPos;

        transform.eulerAngles = new Vector3(-pitch, yaw, 0);
        transform.position = newPos;

        playerState.camYaw = yaw;
        playerState.camPitch = pitch;
        playerState.camPos = newPos;
        playerState.camRot = transform.eulerAngles;

        inputs.camYaw = yaw;
    }

    [SerializeField] private float transitionSpeed = 5;

    public bool TransitionCamera(Vector3 startCamPos, Vector3 targetCamPos, Vector3 startCamRot, Vector3 targetCamRot, float delta)
    {
        /*float distance = Mathf.Max(Vector3.Distance(startCamPos, targetCamPos), Single.Epsilon);

        var position = transform.position;
        
        float t = 1 - Mathf.Clamp01(Vector2.Distance(new Vector2(position.x, position.z), new Vector2(targetCamPos.x, targetCamPos.z)) / distance);
        
        Vector3 spherize = Vector3.up*(Math.Min(distance, 5f)*Mathf.Sin(t*Mathf.PI));
        
        position = Vector3.MoveTowards(position, targetCamPos+spherize, delta*transitionSpeed);
        transform.position = position;

        transform.eulerAngles = Vector3.Slerp(startCamRot, targetCamRot, t);

        return t >= 1;*/
        
        float distance = Mathf.Max(Vector3.Distance(startCamPos, targetCamPos), Single.Epsilon);
        var position = transform.position;

        float t = 1 - Mathf.Clamp01(Vector3.Distance(position, targetCamPos)/distance);

        position = Vector3.MoveTowards(position, targetCamPos, transitionSpeed * delta);
        transform.position = position;
        transform.eulerAngles = Vector3.Slerp(startCamRot, targetCamRot, t);

        return t >= 1;
    }
}

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

    public void UpdateCamera(ref WormController.PlayerState playerState, ref PlayerInput.InputAction inputs)
    {
        float deltaTime = Time.deltaTime;//inputs.deltaTime;

        //ref var inputs = ref playerState.input;
        ref var yaw = ref playerState.camYaw;
        ref var pitch = ref playerState.camPitch;

        yaw += inputs.cameraInput.x * deltaTime * turnSpeed.x;
        pitch -= inputs.cameraInput.y * deltaTime * turnSpeed.y;

        yaw %= 360;
        pitch %= 180;

        transform.eulerAngles = new Vector3(-pitch, yaw, 0);

        Vector3 camPos = new Vector3(
            Mathf.Sin(yaw * Mathf.Deg2Rad) * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad),
            Mathf.Sin(pitch * Mathf.Deg2Rad) * camOffset,
            Mathf.Cos(yaw * Mathf.Deg2Rad) * camOffset * Mathf.Cos(pitch * Mathf.Deg2Rad));

        transform.position = playerState.Transform.position + camPos;

        playerState.camPos = transform.position;
        playerState.camRot = transform.eulerAngles;

        inputs.camYaw = yaw;
    }

    [SerializeField] private float transitionSpeed = 5;

    public bool TransitionCamera(Vector3 startCamPos, Vector3 targetCamPos, Vector3 startCamRot, Vector3 targetCamRot, float delta)
    {
        float distance = Mathf.Max(Vector3.Distance(startCamPos, targetCamPos), Single.Epsilon);

        var position = transform.position;
        
        float t = 1 - Mathf.Clamp01(Vector2.Distance(new Vector2(position.x, position.z), new Vector2(targetCamPos.x, targetCamPos.z)) / distance);
        
        Vector3 spherize = Vector3.up*(Math.Min(distance, 5f)*Mathf.Sin(t*Mathf.PI));
        
        position = Vector3.MoveTowards(position, targetCamPos+spherize, delta*transitionSpeed);
        transform.position = position;

        transform.eulerAngles = Vector3.Slerp(startCamRot, targetCamRot, t);

        return t >= 1;
    }
}

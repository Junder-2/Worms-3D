using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private InputsActions _input;
    
    public struct InputAction
    {
        public Vector2 rawMoveInput;
        
        public Vector2 moveInput;
        public Vector2 cameraInput;
        
        public byte aInput;
        public byte bInput;
        public byte xInput;

        public float camYaw;

        public bool moveNonZero;

        //public float deltaTime;
        //public float fixedDeltatime;
    }

    private void OnEnable()
    {
        _input.Actions.Enable();
    }

    private void OnDisable()
    {
        _input.Actions.Disable();
    }

    private void Awake()
    {
        _input = new InputsActions();

        var action = _input.Actions;

        action.AInput.started += ctx => _aInput = true;
        action.AInput.canceled += ctx => _aInput = false;
        
        action.BInput.started += ctx => _bInput = true;
        action.BInput.canceled += ctx => _bInput = false;

        action.XInput.started += ctx => _xInput = true;
        action.XInput.canceled += ctx => _xInput = false;

        action.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        action.Move.canceled += ctx => _moveInput = Vector2.zero;
        
        action.Camera.performed += ctx => _cameraInput = ctx.ReadValue<Vector2>();
        action.Camera.canceled += ctx => _cameraInput = Vector2.zero;
    }

    private bool _aInput;
    private bool _bInput;
    private bool _xInput;

    private Vector2 _moveInput;
    private Vector2 _cameraInput;

    public void UpdateInputs(ref InputAction input)
    {
        float camYaw = input.camYaw * Mathf.Deg2Rad;
        float camCos = Mathf.Cos(camYaw);
        float camSin = Mathf.Sin(camYaw);

        input.rawMoveInput = _moveInput;

        input.moveNonZero = _moveInput.magnitude > .1f;

        input.moveInput = new Vector2(_moveInput.x * camCos + _moveInput.y * camSin,
            -_moveInput.x * camSin + _moveInput.y * camCos);

        input.cameraInput = _cameraInput;
        
        if (_aInput && input.aInput == 0)
            input.aInput = 1;
        else if (_aInput)
            input.aInput = 2;
        else
            input.aInput = 0;

        if (_bInput && input.bInput == 0)
            input.bInput = 1;
        else if (_bInput)
            input.bInput = 2;
        else
            input.bInput = 0;
        
        if (_xInput && input.xInput == 0)
            input.xInput = 1;
        else if (_xInput)
            input.xInput = 2;
        else
            input.xInput = 0;
    }

}

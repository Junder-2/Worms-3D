using UnityEngine;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        private InputsActions _input;
    
        public struct InputAction
        {
            public Vector2 RawMoveInput;
        
            public Vector2 MoveInput;
            public Vector2 CameraInput;
        
            public byte AInput;
            public byte BInput;
            public byte XInput;

            public float CamYaw;

            public bool MoveNonZero;
            public bool CamNonZero;
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

            action.AInput.started += _ => _aInput = true;
            action.AInput.canceled += _ => _aInput = false;
        
            action.BInput.started += _ => _bInput = true;
            action.BInput.canceled += _ => _bInput = false;

            action.XInput.started += _ => _xInput = true;
            action.XInput.canceled += _ => _xInput = false;

            action.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            action.Move.canceled += _ => _moveInput = Vector2.zero;
        
            action.Camera.performed += ctx => _cameraInput = ctx.ReadValue<Vector2>();
            action.Camera.canceled += _ => _cameraInput = Vector2.zero;
        }

        private bool _aInput;
        private bool _bInput;
        private bool _xInput;

        private Vector2 _moveInput;
        private Vector2 _cameraInput;

        public void UpdateInputs(ref InputAction input)
        {
            float camYaw = input.CamYaw * Mathf.Deg2Rad;
            float camCos = Mathf.Cos(camYaw);
            float camSin = Mathf.Sin(camYaw);

            input.RawMoveInput = _moveInput;

            input.MoveNonZero = _moveInput.magnitude > .1f;
            input.CamNonZero = _cameraInput.magnitude > .1f;

            input.MoveInput = new Vector2(_moveInput.x * camCos + _moveInput.y * camSin,
                -_moveInput.x * camSin + _moveInput.y * camCos);

            input.CameraInput = _cameraInput;
        
            if (_aInput && input.AInput == 0)
                input.AInput = 1;
            else if (_aInput)
                input.AInput = 2;
            else
                input.AInput = 0;

            if (_bInput && input.BInput == 0)
                input.BInput = 1;
            else if (_bInput)
                input.BInput = 2;
            else
                input.BInput = 0;
        
            if (_xInput && input.XInput == 0)
                input.XInput = 1;
            else if (_xInput)
                input.XInput = 2;
            else
                input.XInput = 0;
        }
    }
}

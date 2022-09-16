using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour
{
    public struct PlayerState
    {
        public Transform Transform;
        
        public float camYaw, camPitch;

        public Vector3 camPos, camRot;

        public float maxMoveSpeed;
        public float jumpHeight;

        public bool alive;
    }

    public PlayerState State;
    
    [SerializeField] 
        private MeshRenderer _renderer;

    [SerializeField] 
        private Transform model;

    private Rigidbody rb;

    [SerializeField] private Material[] playerMat;

    public void SetPlayer(int num)
    {
        _renderer.material = playerMat[num];
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _grounded = true;
        _forwardVector = Vector3.forward;
        _normalVector = Vector3.up;

        UpdateInput(new PlayerInput.InputAction());
    }
    
    public enum WormState{idle, moving, jump, freefall}

    private WormState _previousState;
    public WormState _currentState;

    private float _stateTimer;

    private Vector3 _normalVector;
    private Vector3 _forwardVector;
    private float floorLevel;
    private float steepness;

    void SetState(WormState state)
    {
        if (_currentState == WormState.moving && state != WormState.jump)
            forwardVel = 0;

        if (state == WormState.jump)
        {
            _grounded = false;
            RotateModel(Quaternion.LookRotation(_forwardVector, Vector3.up), true);
            Vector3 vel = rb.velocity;
            vel.y = State.jumpHeight;
            rb.velocity = vel;
            _holdJump = true;
        }

        _bonk = false;
        
        _previousState = _currentState;

        _currentState = state;

        _stateTimer = 0;
    }

    private PlayerInput.InputAction _input;
    public void UpdateInput(PlayerInput.InputAction input)
    {
        _input = input;
    }

    private bool _holdJump;
    private bool _bonk;

    void RotateModel(Quaternion target, bool instant = false)
    {
        model.rotation = instant ? Quaternion.RotateTowards(model.rotation, target, _deltaTime * 15f) : target;
    }

    bool CalcGrounded(ref Vector3 position)
    {
        RaycastHit hit;

        if (Physics.Raycast(position + Vector3.up * .5f, Vector3.down, out hit, 100))
        {
            float floorY = hit.point.y;
            
            floorLevel = floorY;
            
            _normalVector = hit.normal;
            steepness = Mathf.Sqrt(_normalVector.x * _normalVector.x + _normalVector.z * _normalVector.z);

            float offset = _grounded ? .1f : 0;

            if (position.y < floorY + offset)
            {
                position.y = floorLevel;
                return true;
            }

            return false;
        }

        return false;
    }

    void GroundPhysicsStep()
    {
        RaycastHit hit;

        Vector3 oldPos = rb.position;
        
        Vector3 pos = oldPos+rb.velocity*_deltaTime;

        _grounded = CalcGrounded(ref pos);

        Vector3 dir = (pos - oldPos);

        bool collision = rb.SweepTest(dir.normalized, out hit, dir.magnitude);
        //transform.forward = _forwardVector;
        //transform.up = _normalVector;
        
        if(collision)
        {
            Vector3 flatHit = new Vector3(hit.point.x, 0, hit.point.z);
            Vector3 flatPos = new Vector3(pos.x, 0, pos.z);
            Vector3 flatNormal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
            
            Debug.DrawRay(hit.point, flatNormal, Color.blue);

            float dist = Vector3.Distance(flatHit, flatPos);

            if (hit.point.y > floorLevel && dist < .5f)
            {
                pos += flatNormal * dist;
            }
        }
        
        rb.MovePosition(pos);
    }

    void AirPhysicsStep()
    {
        RaycastHit hit;
        
        Vector3 oldPos = rb.position;

        Vector3 pos = oldPos + rb.velocity * _deltaTime;
        
        _grounded = CalcGrounded(ref pos);
        
        Vector3 dir = (pos - oldPos);

        bool collision = rb.SweepTest(dir.normalized, out hit, dir.magnitude);
        
        print(collision);
        
        if (collision)
        {
            Vector3 vel = rb.velocity;

            Vector3 flatHit = new Vector3(hit.point.x, 0, hit.point.z);
            Vector3 flatPos = new Vector3(pos.x, 0, pos.z);
            float dist = Vector3.Distance(flatHit, flatPos);
            //float dist = Vector3.Distance(pos+.5f*Vector3.up, hit.point);
            
            if (hit.point.y > floorLevel && dist < .5f)
            {
                print("hello?");
                
                pos += hit.normal * (dist);

                _bonk = true;
            }
            else if(dist < .5f)
            {
                print("hello?==");
                vel.x = 0;
                vel.z = 0;

                rb.velocity = vel;
                
                pos += hit.normal * (dist);

                _bonk = true;
            }
        }

        rb.MovePosition(pos);
    }

    private float _deltaTime;
    private void FixedUpdate()
    {
        _deltaTime = Time.fixedDeltaTime;
        UpdateCharacter();
    }

    public void UpdateCharacter()
    {
        switch (_currentState)
        {
            case WormState.idle:
                IdleState();
                break;
            case WormState.moving:
                MoveState();
                break;
            case WormState.jump:
                JumpState();
                break;
            case WormState.freefall:
                FreeFallState();
                break;
        }
        
        _stateTimer += _deltaTime;
    }

    public bool _grounded;

    void IdleState()
    {
        rb.velocity = Vector3.zero;
        
        GroundPhysicsStep();
        
        RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector));

        if (!_grounded || steepness > .5f)
        {
            SetState(WormState.freefall);
            return;
        }


        if (_input.moveNonZero)
        {
            SetState(WormState.moving);
            return;
        }


        if (_input.aInput == 1)
        {
            SetState(WormState.jump);
            return;
        }
            
    }

    private float forwardVel = 0;

    void MoveState()
    {
        if (!_input.moveNonZero)
        {
            SetState(WormState.idle);
            return;
        }

        Vector3 move = new Vector3(_input.moveInput.x, 0, _input.moveInput.y);
        move = Vector3.ProjectOnPlane(move, _normalVector);
        float maxSpeed = State.maxMoveSpeed;

        _forwardVector = Vector3.MoveTowards(_forwardVector, move, _deltaTime * 5f);

        forwardVel += maxSpeed * .5f * _deltaTime;

        if (forwardVel > maxSpeed)
            forwardVel = maxSpeed;

        rb.velocity = _forwardVector * forwardVel;

        GroundPhysicsStep();

        RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector));

        if (!_grounded || steepness > .5f)
        {
            SetState(WormState.freefall);
            return;
        }

        if (_input.aInput > 0)
        {
            SetState(WormState.jump);
            return;
        }
    }

    private const float terminalVel = -15;

    void ApplyAirForce(float gravityMultiplier)
    {
        Vector3 vel = rb.velocity;
        
        vel.y += Physics.gravity.y * gravityMultiplier*Time.fixedDeltaTime;

        if (vel.y < terminalVel)
            vel.y = terminalVel;
        if(Mathf.Abs(vel.x) > 0)
            vel.x -= vel.x*.25f*_deltaTime;
        if(Mathf.Abs(vel.z) > 0)
            vel.z -= vel.z*.25f*_deltaTime;

        rb.velocity = vel;

        print(vel);
    }
    
    void JumpState()
    {
        if (_stateTimer < .2f)
            _grounded = false;
        
        float multi = 1;

        if (_holdJump && _input.aInput > 1)
            multi = .4f;
        else
        {
            _holdJump = false;
        }
        
        ApplyAirForce(multi);
        AirPhysicsStep();

        if (_grounded)
        {
            SetState(WormState.idle);
            return;
        }

        if (_bonk)
        {
            SetState(WormState.freefall);
            return;
        }
    }

    void FreeFallState()
    {
        ApplyAirForce(1);
        AirPhysicsStep();

        if (_grounded && steepness < .5f)
        {
            SetState(WormState.idle);
            return;
        }
    }
}

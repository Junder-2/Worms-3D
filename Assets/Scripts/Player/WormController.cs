using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour, IEntity
{
    public struct PlayerState
    {
        public Transform Transform;
        
        public float camYaw, camPitch;

        public Vector3 camPos, camRot;

        public float maxMoveSpeed;
        public float jumpHeight;

        public float health;
        public bool alive;

        public Vector3 velocity;

        public Vector3 startPos;
        public float maxDistance;
        public float floorLevel;

        public byte currentWeapon;
    }

    public PlayerState State;
    
    [SerializeField] 
        private Renderer[] _renderer;

    [SerializeField] 
        private Transform model;

    private Rigidbody rb;
    private Animator animator;

    [SerializeField] private Material[] playerMat;

    [SerializeField] private Weapon[] weapons;

    public Vector3 GetForwards()
    {
        return Vector3.ProjectOnPlane(_forwardVector, _normalVector);
    }

    public void SetPlayer(int num)
    {
        for (int i = 0; i < _renderer.Length; i++)
        {
            _renderer[i].material = playerMat[num];
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _grounded = true;
        _forwardVector = Vector3.forward;
        _normalVector = Vector3.up;

        UpdateInput(new PlayerInput.InputAction());
    }
    
    public enum WormState{idle, moving, jump, freefall, meleeAttack}

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

        if(_currentState == WormState.jump)
            forwardVel = 0;

        if (state == WormState.jump)
        {
            _grounded = false;
            RotateModel(Quaternion.LookRotation(_forwardVector, Vector3.up), true);
            State.velocity.y = State.jumpHeight;
            _holdJump = true;
        }
        
        _bonk = false;
        
        _previousState = _currentState;

        _currentState = state;

        _stateTimer = 0;
        _deltaTime = 0;
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
        model.rotation = instant ? target : Quaternion.RotateTowards(model.rotation, target, _deltaTime * 360f);
    }

    bool CalcGrounded(ref Vector3 position)
    {
        RaycastHit hit;

        if (Physics.Raycast(position + Vector3.up * .5f, Vector3.down, out hit, 100))
        {
            float floorY = hit.point.y;
            
            floorLevel = floorY;
            State.floorLevel = floorLevel;
            
            _normalVector = hit.normal;
            steepness = Mathf.Sqrt(_normalVector.x * _normalVector.x + _normalVector.z * _normalVector.z);

            float offset = _grounded ? .1f : 0;

            if (position.y < floorY + offset && steepness < .85f)
            {
                position.y = floorLevel;
                return true;
            }

            return false;
        }

        return false;
    }

    const float hitboxHeight = .3f;

    void GroundPhysicsStep()
    {
        RaycastHit hit;

        Vector3 oldPos = rb.position;
        
        Vector3 pos = oldPos+State.velocity*_deltaTime;

        _grounded = CalcGrounded(ref pos);

        oldPos = new Vector3(oldPos.x, pos.y, oldPos.z);

        Vector3 dir = (pos-oldPos);

        bool collision = Physics.Raycast(oldPos+hitboxHeight*Vector3.up, dir.normalized, out hit, 1f);
        //Physics.SphereCast(oldPos+.5f*Vector3.up, .5f, dir.normalized, out hit, dir.magnitude);
        
        if(collision)
        {
            Vector3 flatNormal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
            
            Debug.DrawRay(hit.point, flatNormal, Color.blue);

            float dist = Vector3.Distance(pos + hitboxHeight * Vector3.up, hit.point);

            if (hit.point.y > floorLevel+hitboxHeight/3 && (dist) <= .5f)
            {
                pos += flatNormal * (.5f-dist);
            }
        }
        
        transform.position = (pos);
    }

    void AirPhysicsStep()
    {
        RaycastHit hit;
        
        Vector3 oldPos = rb.position;

        Vector3 pos = oldPos + State.velocity * _deltaTime;
        
        _grounded = CalcGrounded(ref pos);

        Vector3 dir = (pos - oldPos);

        bool collision = Physics.Raycast(oldPos+hitboxHeight*Vector3.up, dir.normalized, out hit, 1f);

        if(!collision)
        {
            collision = Physics.Raycast(oldPos+hitboxHeight*Vector3.up, Vector3.down, out hit, 1f);
        }
        
        if (collision)
        {
            float dist = Vector3.Distance(pos+hitboxHeight*Vector3.up, hit.point);
            //print(dist + ", " + hit.distance);
            
            if (hit.point.y > floorLevel+hitboxHeight/3 && dist <= .5f)
            {
                print("hello?");
                
                pos += hit.normal * (.5f-dist);

                if(new Vector2(State.velocity.x, State.velocity.z).magnitude > 2f)
                    State.velocity = Vector3.Reflect(State.velocity, hit.normal)/3;

                _bonk = true;
            }
        }

        transform.position = (pos);
    }

    private Weapon _currentWeapon;
    void SwitchWeapon(bool setZero = false)
    {
        if(_currentWeapon != null)_currentWeapon.gameObject.SetActive(false);
        
        ref var curr = ref State.currentWeapon;
        curr++;
        if (setZero)
            curr = 0;
        
        if (weapons.Length < curr)
            curr = 0;

        if (curr == 0) return;
        
        _currentWeapon = weapons[curr - 1];
            
        if(!_currentWeapon.CanEquip())
            SwitchWeapon();
            
        _currentWeapon.gameObject.SetActive(true);

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
            case WormState.meleeAttack:
                MeleeAttackState();
                break;
        }
        
        if(_input.xInput == 1)
            SwitchWeapon();
        
        _stateTimer += _deltaTime;
    }

    public bool _grounded;

    void IdleState()
    {
        if(_stateTimer == 0)
        {
            animator.SetBool("Grounded", true);
            animator.SetBool("Walk", false);

            State.velocity = Vector3.zero;
            
            GroundPhysicsStep();
            
            RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector), true);
        }

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

        if (_input.bInput == 1 && State.currentWeapon != 0)
        {
            if (_currentWeapon.IsMelee())
            {
                SetState(WormState.meleeAttack);
                return;
            }
        }
            
    }

    private float forwardVel = 0;

    void MoveState()
    {
        if(_stateTimer == 0)
        {
            animator.SetBool("Grounded", true);
            animator.SetBool("Walk", true);
        }

        if (!_input.moveNonZero)
        {
            SetState(WormState.idle);
            return;
        }

        Vector3 move = new Vector3(_input.moveInput.x, 0, _input.moveInput.y);
        move = Vector3.ProjectOnPlane(move, _normalVector);
        float maxSpeed = State.maxMoveSpeed;

        _forwardVector = Vector3.MoveTowards(_forwardVector, move, _deltaTime * 5f);

        forwardVel += maxSpeed * .4f * _deltaTime;

        animator.SetFloat("MoveSpeed", forwardVel/maxSpeed);

        if (forwardVel > maxSpeed)
            forwardVel = maxSpeed;

        Vector3 oldPos = transform.position;

        State.velocity = _forwardVector * forwardVel;

        GroundPhysicsStep();

        //if (Vector3.Distance(State.startPos, transform.position) > State.maxDistance)
        //s    transform.position = oldPos;

        RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector));

        if (!_grounded || steepness > .5f)
        {
            SetState(WormState.freefall);
            return;
        }

        if (_input.aInput > 0)
        {
            State.velocity = _forwardVector * Mathf.Max(forwardVel, maxSpeed/2);
            SetState(WormState.jump);
            return;
        }
    }

    private const float terminalVel = -25;

    void ApplyAirForce(float gravityMultiplier)
    {
        Vector3 vel = State.velocity;
        
        vel.y += Physics.gravity.y * gravityMultiplier*Time.fixedDeltaTime;

        if (vel.y < terminalVel)
            vel.y = terminalVel;
        if(Mathf.Abs(vel.x) > 0)
            vel.x -= vel.x*.25f*_deltaTime;
        if(Mathf.Abs(vel.z) > 0)
            vel.z -= vel.z*.25f*_deltaTime;

        State.velocity = vel;
    }
    
    void JumpState()
    {
        if(_stateTimer == 0)
        {
            animator.SetBool("Grounded", false);
            animator.SetTrigger("Jump");
        }

        if (_stateTimer < .2f)
            _grounded = false;
        
        float multi = 1;

        if (_holdJump && _input.aInput > 1 && State.velocity.y > 0)
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
        if(_stateTimer == 0)
        {
            animator.SetBool("Grounded", false);
        }
        ApplyAirForce(1);
        AirPhysicsStep();

        if (_grounded && steepness < .5f)
        {
            SetState(WormState.idle);
            return;
        }
    }

    private float _waitTime;
    void MeleeAttackState()
    {
        if (_stateTimer == 0)
        {
            _waitTime = _currentWeapon.UseWeapon(this);
        }

        if (_stateTimer > _waitTime)
        {
            SetState(WormState.idle);
            return;
        }
    }

    public void SetAnimTrigger(string anim)
    {
        animator.SetTrigger(anim);
    }

    public void DeEquipWeapon()
    {
        SwitchWeapon(true);
    }

    public void Damage(float amount, Vector3 force)
    {
        print("damaged");
        _grounded = false;
        SetState(WormState.freefall);

        State.velocity = force;
        State.health -= amount;
    }

    public Vector3 GetPos()
    {
        return transform.position+hitboxHeight*Vector3.up;
    }
}

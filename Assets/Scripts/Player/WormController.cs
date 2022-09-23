using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public byte wormIndex, playerIndex;

        public Vector3 velocity;

        public Vector3 startPos;

        public bool freezeCamPitch;

        public bool freezeCamYaw;
        //public float maxDistance;
        //public float floorLevel;

        public byte currentWeapon;

        public float currentWaterLevel;

        public bool currentPlayer;
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

    public Vector3 GetUp()
    {
        return _normalVector;
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
        _maxHealth = GameRules.wormsMaxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _grounded = true;
        _forwardVector = Vector3.forward;
        _normalVector = Vector3.up;

        UpdateInput(new PlayerInput.InputAction());
    }
    
    public enum WormState{idle, moving, jump, freefall, slide, attack, death}

    private WormState _previousState;
    public WormState _currentState;

    private float _stateTimer;

    private Vector3 _normalVector;
    private Vector3 _forwardVector;
    private Vector3 _slopeVector;
    private float _floorLevel;
    private float _steepness;

    private float _maxHealth;

    private bool _unlockRotation;
    private Vector3 _rotationVelocity;

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

        State.freezeCamPitch = false;
        State.freezeCamYaw = false;

        _stateTimer = 0;
        _deltaTime = 0;
    }

    private PlayerInput.InputAction _input;
    public void UpdateInput(PlayerInput.InputAction input)
    {
        _input = input;
    }

    public void SetPlayerTurn()
    {
        int[] weaponAmount = new int[weapons.Length];

        for (int i = 0; i < weaponAmount.Length; i++)
        {
            weaponAmount[i] = weapons[i].GetAmount();
        }
        
        UIManager.Instance.InstanceWeaponUI(weaponAmount, State.currentWeapon-1);
    }

    public PlayerInput.InputAction GetInput()
    {
        return _input;
    }

    private bool _holdJump;
    private bool _bonk;

    public void TurnPlayer(float amount)
    {
        _forwardVector = Quaternion.Euler(0, amount, 0)*_forwardVector;
        RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector), true);
    }

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
            
            _floorLevel = floorY;
            //State.floorLevel = _floorLevel;
            
            _normalVector = hit.normal;
            _steepness = Mathf.Sqrt(_normalVector.x * _normalVector.x + _normalVector.z * _normalVector.z);
            
            Vector3 acrossSlope = Vector3.Cross(Vector3.up, _normalVector);
            _slopeVector = Vector3.Cross(acrossSlope, _normalVector);

            float offset = _grounded ? .1f : 0;

            if (position.y < floorY + offset && _steepness < .95f)
            {
                position.y = _floorLevel;
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

            if (hit.point.y > _floorLevel+hitboxHeight/3 && (dist) <= .5f)
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
            
            if (hit.point.y > _floorLevel+.1f && dist <= .5f)
            {
                print("hello?");
                
                pos += hit.normal * (.5f-dist);
                pos -= _slopeVector * (.5f * _deltaTime * Physics.gravity.y * _steepness);

                if(new Vector2(State.velocity.x, State.velocity.z).magnitude > 5f)
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
        print(curr);
        
        if (setZero)
            curr = 0;
        
        if (weapons.Length < curr)
            curr = 0;

        if (curr == 0)
        {
            UIManager.Instance.UpdateWeaponUI(-1, 0);
            return;
        }
        
        _currentWeapon = weapons[curr - 1];

        if (!_currentWeapon.CanEquip())
        {
            SwitchWeapon();
            return;
        }

        _currentWeapon.gameObject.SetActive(true);
        
        UIManager.Instance.UpdateWeaponUI(curr-1, _currentWeapon.GetAmount());
        
        print(curr);
    }

    private float _deltaTime;
    private void FixedUpdate()
    {
        _deltaTime = Time.fixedDeltaTime;
        UpdateCharacter();
    }

    private void UpdateCharacter()
    {
        if (transform.position.y + hitboxHeight < State.currentWaterLevel)
        {
            Damage(9999, Vector3.zero);
        }
        
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
            case WormState.slide:
                SlideState();
                break;
            case WormState.attack:
                AttackState();
                break;
            case WormState.death:
                DeathState();
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
            _unlockRotation = false;
            _rotationVelocity = Vector3.zero;
            animator.SetBool("Grounded", true);
            animator.SetBool("Walk", false);

            State.velocity = Vector3.zero;
            
            GroundPhysicsStep();
            
            RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector), true);
        }

        if (!_grounded)
        {
            SetState(WormState.freefall);
            return;
        }

        if (_steepness > .5f)
        {
            SetState(WormState.slide);
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
            SetState(WormState.attack);
            return;
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

        if (!_grounded)
        {
            SetState(WormState.freefall);
            return;
        }
        
        if (_steepness > .5f)
        {
            SetState(WormState.slide);
            return;
        }

        if (_input.aInput > 0)
        {
            State.velocity = _forwardVector * Mathf.Max(forwardVel, maxSpeed/2);
            SetState(WormState.jump);
            return;
        }
    }

    void SlideState()
    {
        State.velocity -= _slopeVector * (.5f * _deltaTime * Physics.gravity.y * _steepness);

        GroundPhysicsStep();
        
        RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector));
        
        if (!_grounded)
        {
            SetState(WormState.freefall);
            return;
        }
        
        if (_steepness < .5f)
        {
            SetState(WormState.idle);
            return;
        }
    }

    private const float terminalVel = -20;

    void ApplyAirForce(float gravityMultiplier)
    {
        Vector3 vel = State.velocity;
        
        vel.y += Physics.gravity.y * gravityMultiplier*_deltaTime;

        if (vel.y < terminalVel)
            vel.y = terminalVel;
        if(Mathf.Abs(vel.x) > 0)
            vel.x -= vel.x*.25f*_deltaTime;
        if(Mathf.Abs(vel.z) > 0)
            vel.z -= vel.z*.25f*_deltaTime;
        
        _rotationVelocity = Vector3.MoveTowards(_rotationVelocity, Vector3.zero, _deltaTime/2f);

        if (_unlockRotation)
            model.rotation *= Quaternion.Euler(_rotationVelocity * _deltaTime);

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

        if (_grounded)
        {
            SetState(WormState.idle);
            return;
        }
    }

    public void StopAttackWait() => _attackWait = false;
    private bool _attackWait;
    void AttackState()
    {
        if (_stateTimer == 0)
        {
            _attackWait = true;
            _currentWeapon.UseWeapon(this);
            return;
        }

        if (!_attackWait)
        {
            UIManager.Instance.UpdateWeaponUI(State.currentWeapon-1, _currentWeapon.GetAmount());
            SetState(WormState.idle);
            return;
        }
    }

    void DeathState()
    {
        State.alive = false;
        if(State.currentPlayer)ForceEndTurn();
        gameObject.SetActive(false);
    }

    public void ForceEndTurn()
    {
        LevelController.Instance.ForceTurnEnd();
    }

    public void SetAnimTrigger(string anim)
    {
        animator.SetTrigger(anim);
    }

    public void DeEquipWeapon()
    {
        UIManager.Instance.UpdateWeaponUI(State.currentWeapon-1, _currentWeapon.GetAmount());
        SwitchWeapon(true);
    }

    public void CancelAction()
    {
        if(_currentWeapon != null)
            _currentWeapon.CancelWeapon(this);
    }

    public void Damage(float amount, Vector3 force)
    {
        print("damaged");
        _grounded = false;
        _unlockRotation = true;
        Vector3 angles = Quaternion.LookRotation(force.normalized, Vector3.up).eulerAngles*(force.magnitude/5);
        angles.x *= MathHelper.RandomSign();
        angles.y *= MathHelper.RandomSign();
        angles.z *= MathHelper.RandomSign();

        _rotationVelocity = angles;
        SetState(WormState.freefall);

        State.velocity += force;
        State.health = Mathf.Max(State.health - amount, 0);
        
        UIManager.Instance.UpdatePlayerHealth(State.playerIndex, State.wormIndex, State.health/_maxHealth);
        
        if(State.health <= 0)
            SetState(WormState.death);
    }

    public Vector3 GetPos()
    {
        return transform.position+hitboxHeight*Vector3.up;
    }
}

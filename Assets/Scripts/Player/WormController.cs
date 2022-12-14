using Managers;
using UI;
using UnityEngine;
using Weapons;
using Random = UnityEngine.Random;

namespace Player
{
    [RequireComponent(typeof(WormsEffects))]
    public class WormController : MonoBehaviour, IEntity
    {
        public PlayerInfo.WormState State;

        [HideInInspector] public WormsEffects effects;

        [SerializeField] 
        private Transform model;

        [SerializeField] 
        private Weapon[] weapons;

        private Rigidbody _rb;
    
        private Vector3 _normalVector;
        private Vector3 _forwardVector;
        private Vector3 _slopeVector;
        private float _floorLevel;
        private float _steepness;
        private float _forwardVel = 0;

        private float _maxHealth;
        private bool _holdJump;
        private bool _bonk;
        private bool _grounded;

        private bool _unlockRotation;
        private Vector3 _rotationVelocity;
    
        private void Awake()
        {
            _maxHealth = GameRules.WormsMaxHealth;
            _rb = GetComponent<Rigidbody>();
            _grounded = true;
            _forwardVector = Vector3.forward;
            _normalVector = Vector3.up;

            effects = GetComponent<WormsEffects>();

            UpdateInput(new PlayerInput.InputAction());
        }
    
        private float _deltaTime;
        private void FixedUpdate()
        {
            _deltaTime = Time.fixedDeltaTime;
            effects.BlinkLoop();
            UpdateCharacterState();
        }

        private PlayerInput.InputAction _input;
        public void UpdateInput(PlayerInput.InputAction input) => _input = input;

        public PlayerInput.InputAction GetInput() => _input;

        private Weapon _currentWeapon;

        private void SwitchWeapon(bool setZero = false)
        {
            if(_attackWait)
                return;
        
            if(State.CurrentWeapon != 0)_currentWeapon.gameObject.SetActive(false);
        
            ref var curr = ref State.CurrentWeapon;
            curr++;

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
        }
    
        public void DeEquipWeapon()
        {
            UIManager.Instance.UpdateWeaponUI(State.CurrentWeapon-1, _currentWeapon.GetAmount());
            SwitchWeapon(true);
        }
    
        public int[] GetWeaponsAmount(bool instantiate = false)
        {
            int[] weaponAmount = new int[weapons.Length];
        
            for (int i = 0; i < weaponAmount.Length; i++)
            {
                weaponAmount[i] = instantiate ? weapons[i].GetBaseAmount() : weapons[i].GetAmount();
            }

            return weaponAmount;
        }

        public void SetPlayerTurn(PlayerInfo.PlayerData data)
        {
            State.CamFollow = transform;

            for (int i = 0; i < data.WeaponAmount.Length; i++)
            {
                weapons[i].SetAmount((byte)data.WeaponAmount[i]);
            }

            UIManager.Instance.InstanceWeaponUI(data.WeaponAmount, State.CurrentWeapon-1);
        }
    
        public void TurnPlayer(float amount)
        {
            _forwardVector = Quaternion.Euler(0, amount, 0)*_forwardVector;
            RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector), true);
        }

        void RotateModel(Quaternion target, bool instant = false)
        {
            model.rotation = instant ? target : Quaternion.RotateTowards(model.rotation, target, _deltaTime * 360f);
        }

        #region StateMachine

        private enum ActionState{idle, moving, jump, freefall, slide, attack, death}

        private ActionState _previousState;
        private ActionState _currentState;

        private float _stateTimer;

        private void SetState(ActionState state)
        {
            if (_currentState == ActionState.moving && state != ActionState.jump)
                _forwardVel = 0;

            if(_currentState == ActionState.jump)
                _forwardVel = 0;

            if (state == ActionState.jump)
            {
                _grounded = false;
                RotateModel(Quaternion.LookRotation(_forwardVector, Vector3.up), true);
                State.Velocity.y = State.JumpHeight;
                _holdJump = true;
            }
        
            _previousState = _currentState;

            _currentState = state;

            State.FreezeCamPitch = false;
            State.FreezeCamYaw = false;
            effects.SetSmokeParticles(false);
            _bonk = false;

            _stateTimer = 0;
            _deltaTime = 0;
        }

        void IdleState()
        {
            if(_stateTimer == 0)
            {
                _unlockRotation = false;
                _rotationVelocity = Vector3.zero;
                effects.SetAnimBool("Grounded", true);
                effects.SetAnimBool("Walk", false);

                State.Velocity = Vector3.zero;
            
                GroundPhysicsStep();
            
                RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector), true);
            }

            if (!_grounded)
            {
                SetState(ActionState.freefall);
                return;
            }

            if (_steepness > PlayerInfo.SlopeLimit)
            {
                SetState(ActionState.slide);
                return;
            }

            if (_input.MoveNonZero)
            {
                SetState(ActionState.moving);
                return;
            }

            if (_input.AInput == 1)
            {
                SetState(ActionState.jump);
                return;
            }

            if (_input.BInput == 1 && State.CurrentWeapon != 0)
            {
                SetState(ActionState.attack);
                return;
            }
        
            if(_input.XInput == 1)
                SwitchWeapon();
            
        }
    
        void MoveState()
        {
            if(_stateTimer == 0)
            {
                effects.SetAnimBool("Grounded", true);
                effects.SetAnimBool("Walk", true);
            }

            if (!_input.MoveNonZero)
            {
                SetState(ActionState.idle);
                return;
            }

            Vector3 move = new Vector3(_input.MoveInput.x, 0, _input.MoveInput.y);
            move = Vector3.ProjectOnPlane(move, _normalVector);
            float maxSpeed = State.MaxMoveSpeed;

            _forwardVector = Vector3.MoveTowards(_forwardVector, move, _deltaTime * 5f);

            _forwardVel += maxSpeed * .4f * _deltaTime;

            effects.SetAnimFloat("MoveSpeed", _forwardVel/maxSpeed);

            if (_forwardVel > maxSpeed)
                _forwardVel = maxSpeed;

            Vector3 oldPos = transform.position;

            State.Velocity = _forwardVector * _forwardVel;

            GroundPhysicsStep();

            RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector));

            if (!_grounded)
            {
                SetState(ActionState.freefall);
                return;
            }
        
            if (_steepness > PlayerInfo.SlopeLimit)
            {
                SetState(ActionState.slide);
                return;
            }

            if (_input.AInput > 0)
            {
                State.Velocity = _forwardVector * Mathf.Max(_forwardVel, maxSpeed/2);
                SetState(ActionState.jump);
                return;
            }
        
            if(_input.XInput == 1)
                SwitchWeapon();
        }

        void SlideState()
        {
            State.Velocity -= _slopeVector * (.5f * _deltaTime * Physics.gravity.y * _steepness);

            GroundPhysicsStep();
        
            RotateModel(Quaternion.LookRotation(_forwardVector, _normalVector));
        
            if (!_grounded)
            {
                SetState(ActionState.freefall);
                return;
            }
        
            if (_steepness < PlayerInfo.SlopeLimit-.15f)
            {
                SetState(ActionState.idle);
                return;
            }
        
            if(_input.XInput == 1)
                SwitchWeapon();
        }

        void JumpState()
        {
            if(_stateTimer == 0)
            {
                effects.SetAnimBool("Grounded", false);
                effects.SetAnimTrigger("Jump");

                effects.PlaySound((int)AudioSet.AudioID.hiya2);
            }

            if (_stateTimer < .2f)
                _grounded = false;
        
            float multi = 1;

            if (_holdJump && _input.AInput > 1 && State.Velocity.y > 0)
                multi = .4f;
            else
            {
                _holdJump = false;
            }
        
            ApplyAirForce(multi);
            AirPhysicsStep();

            if (_grounded)
            {
                SetState(_steepness < PlayerInfo.SlopeLimit - .1f ? ActionState.idle : ActionState.slide);
                return;
            }

            if (_bonk)
            {
                SetState(ActionState.freefall);
                return;
            }
        
            if(_input.XInput == 1)
                SwitchWeapon();
        }

        void FreeFallState()
        {
            if(_stateTimer == 0)
            {
                effects.SetAnimBool("Grounded", false);
            }
            ApplyAirForce(1);
            AirPhysicsStep();

            if (_grounded)
            {
                SetState(_steepness < PlayerInfo.SlopeLimit - .1f ? ActionState.idle : ActionState.slide);
                return;
            }
        
            if(_input.XInput == 1)
                SwitchWeapon();
        }
    
        void AttackState()
        {
            if (_stateTimer == 0)
            {
                _attackWait = true;
                _currentWeapon.UseWeapon(this);
                return;
            }

            if (_attackWait) return;
            UIManager.Instance.UpdateWeaponUI(State.CurrentWeapon-1, _currentWeapon.GetAmount());
            SetState(ActionState.idle);
            return;
        }

        void DeathState()
        {
            State.Alive = false;
            if(State.CurrentPlayer)ForceEndTurn();
            LevelController.Instance.ProcessDeath();
        
            gameObject.SetActive(false);
        }
    
        private void UpdateCharacterState()
        {
            if (transform.position.y + PlayerInfo.HitboxHeight < State.CurrentWaterLevel)
            {
                LevelEffects.Instance.SpawnWaterSplash(transform.position);
                Damage(9999, Vector3.zero);
            }
        
            switch (_currentState)
            {
                case ActionState.idle:
                    IdleState();
                    break;
                case ActionState.moving:
                    MoveState();
                    break;
                case ActionState.jump:
                    JumpState();
                    break;
                case ActionState.freefall:
                    FreeFallState();
                    break;
                case ActionState.slide:
                    SlideState();
                    break;
                case ActionState.attack:
                    AttackState();
                    break;
                case ActionState.death:
                    DeathState();
                    break;
            }

            _stateTimer += _deltaTime;
        }
    
        #endregion
    
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

        void GroundPhysicsStep()
        {
            RaycastHit hit;

            Vector3 oldPos = _rb.position;
        
            Vector3 pos = oldPos+State.Velocity*_deltaTime;

            _grounded = CalcGrounded(ref pos);

            oldPos = new Vector3(oldPos.x, pos.y, oldPos.z);

            Vector3 dir = (pos-oldPos);

            bool collision = Physics.Raycast(oldPos+PlayerInfo.HitboxHeight*Vector3.up, dir.normalized, out hit, 1f);

            if(collision)
            {
                Vector3 flatNormal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
            
                //Debug.DrawRay(hit.point, flatNormal, Color.blue);

                float dist = Vector3.Distance(pos + PlayerInfo.HitboxHeight * Vector3.up, hit.point);

                if (hit.point.y > _floorLevel+PlayerInfo.HitboxHeight/3 && (dist) <= .5f)
                    pos += flatNormal * (.5f-dist);
            }
        
            transform.position = (pos);
        }

        void AirPhysicsStep()
        {
            RaycastHit hit;
        
            Vector3 oldPos = _rb.position;

            Vector3 pos = oldPos + State.Velocity * _deltaTime;
        
            _grounded = CalcGrounded(ref pos);

            bool collision = Physics.Raycast(oldPos+PlayerInfo.HitboxHeight*Vector3.up, (pos - oldPos).normalized, out hit, 1f);

            if(!collision)
                collision = Physics.Raycast(oldPos+PlayerInfo.HitboxHeight*Vector3.up, Vector3.down, out hit, 1f);

            if (collision)
            {
                float dist = Vector3.Distance(pos+PlayerInfo.HitboxHeight*Vector3.up, hit.point);
                //print(dist + ", " + hit.distance);
            
                if (hit.point.y > _floorLevel+.1f && dist <= .5f)
                {
                    pos += hit.normal * (.5f-dist);
                    pos -= _slopeVector * (.5f * _deltaTime * Physics.gravity.y * _steepness);

                    if(new Vector2(State.Velocity.x, State.Velocity.z).magnitude > 5f)
                        State.Velocity = Vector3.Reflect(State.Velocity, hit.normal)/3;

                    _bonk = true;
                }
                else if (hit.point.y < _floorLevel && dist <= .5f)
                {
                    pos += hit.normal * (.5f-dist);
                    pos -= _slopeVector * (.5f * _deltaTime * Physics.gravity.y * _steepness);
                }
            }

            transform.position = (pos);
        }
    
        void ApplyAirForce(float gravityMultiplier)
        {
            Vector3 vel = State.Velocity;
        
            vel.y += Physics.gravity.y * gravityMultiplier*_deltaTime;

            if (vel.y < PlayerInfo.TerminalVel)
                vel.y = PlayerInfo.TerminalVel;
            if(Mathf.Abs(vel.x) > 0)
                vel.x -= vel.x*.25f*_deltaTime;
            if(Mathf.Abs(vel.z) > 0)
                vel.z -= vel.z*.25f*_deltaTime;
        
            _rotationVelocity = Vector3.MoveTowards(_rotationVelocity, Vector3.zero, _deltaTime/2f);

            if (_unlockRotation)
                model.rotation *= Quaternion.Euler(_rotationVelocity * _deltaTime);

            State.Velocity = vel;
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

            CancelAction();
            _rotationVelocity = angles;
            SetState(ActionState.freefall);

            if(force.magnitude < 5f)
            {
                int clip = Random.Range(0, 2) == 0 ? (int)AudioSet.AudioID.ow : (int)AudioSet.AudioID.oww;
            
                effects.PlaySound(clip);
            }
            else
            {
                effects.PlaySound((int)AudioSet.AudioID.waah);
                effects.SetSmokeParticles(true);
            }
       

            State.Velocity += force;
            State.Health = Mathf.Max(State.Health - amount, 0);
        
            UIManager.Instance.UpdatePlayerHealth(State.PlayerIndex, State.WormIndex, State.Health/_maxHealth);
            effects.SetHealthUI(State.Health/_maxHealth);

            if(State.Health <= 0)
                SetState(ActionState.death);
        }
    
        public void StopAttackWait()
        {
            _attackWait = false;
            SetCamZoom(1);
        } 
        private bool _attackWait;

        private void ForceEndTurn() => LevelController.Instance.ForceTurnEnd();

        public void SetCamZoom(float value)
        {
            value = Mathf.Max(0, value);
        
            State.CamZoom = value;
        }

        public void CancelAction()
        {
            SetCamZoom(1);

            UpdateInput(new PlayerInput.InputAction());
        
            if(_currentState == ActionState.attack) effects.PlaySound((int)AudioSet.AudioID.hmmrgh);
        
            if(State.CurrentWeapon != 0)
                _currentWeapon.CancelWeapon(this);
        }

        public Vector3 GetPos() => transform.position+PlayerInfo.HitboxHeight*Vector3.up;
        public Vector3 GetForwards() => Vector3.ProjectOnPlane(_forwardVector, _normalVector);
        public Vector3 GetUp() => _normalVector;
    }
}

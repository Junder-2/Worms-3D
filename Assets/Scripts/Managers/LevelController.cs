using Player;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Managers
{
    [RequireComponent(typeof(PlayerInput))]
    public class LevelController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private CameraController _cameraController;

        public static LevelController Instance;

        [SerializeField] private GameObject playerPrefab;

        private float _roundTimer = 30;

        private PlayerInput.InputAction _input;

        private byte _wormsPerPlayer;
        private byte _playerAmount;
        private byte _lastPlayer = 5;
        private byte _currentPlayer;

        private byte _currentWorm;
        private byte _lastWorm;

        private float _maxHealth;

        void NextPlayer(byte currPlayer)
        {
            _currentWormController.UpdateInput(new PlayerInput.InputAction());
            _currentWormController.State.CurrentPlayer = false;

            ProcessDeath();

            byte potentialPlayer = (byte)((currPlayer + 1) % _playerAmount);
        
            GetNextWorm(potentialPlayer);
        }

        void GetNextWorm(byte player)
        {
            int newWorm = 0;
            bool fail = true;

            int[] wormList = PlayerData[player].Worms;
        
            MathHelper.ShuffleArray(ref wormList);
        
            for (int i = 0; i < _wormsPerPlayer; i++)
            {
                newWorm = wormList[i];

                if (!_wormsControllers[newWorm].State.Alive) continue;
            
                fail = false;
                break;
            }

            if (fail)
            {
                NextPlayer(player);
                return;
            }
        
            _lastWorm = _currentWorm;
            _lastPlayer = _currentPlayer;
            _currentPlayer = player;

            _currentWorm = (byte)newWorm;
            _currentWormController = _wormsControllers[_currentWorm];
        
            _currentWormController.State.CurrentPlayer = true;
        
            _currentWormController.SetPlayerTurn(PlayerData[_currentPlayer]);
        }

        private WormController[] _wormsControllers;

        private WormController _currentWormController;

        public PlayerInfo.PlayerData[] PlayerData;

        private enum LevelState {startGame, playerControl, turnEnd, playerWin}

        private LevelState _levelState;
        private LevelState _lastLevelState;
    
        private float _stateTimer;
        private bool _deltaPause;

        private void SetState(LevelState newState)
        {
            if (newState == LevelState.playerControl)
            {
                UIManager.Instance.StartTimerUI(_roundTimer, true);
            }
        
            _lastLevelState = _levelState;

            _levelState = newState;

            _stateTimer = 0;

            _endTurn = false;

            //_input.deltaTime = 0;
            _deltaPause = true;
        }

        private void Start()
        {
            Instance = this;
        
            _playerAmount = GameRules.PlayerAmount;
            _wormsPerPlayer = GameRules.WormsPerPlayer;
            _roundTimer = GameRules.RoundTimer;
        
            _playerInput = GetComponent<PlayerInput>();
            _cameraController = CameraController.Instance;

            AudioManager.Instance.PlayMusic((int)AudioSet.MusicID.game);

            SetState(LevelState.startGame);
        }

        private void FixedUpdate()
        {
            _playerInput.UpdateInputs(ref _input);
        }

        private void Update()
        {
            //_input.deltaTime = Time.deltaTime *simulationSpeed;

            switch (_levelState)
            {
                case LevelState.startGame:
                    StartGameState();
                    break;
                case LevelState.playerControl:
                    PlayerControlState();
                    break;
                case LevelState.turnEnd:
                    EndTurnState();
                    break;
                case LevelState.playerWin:
                    PlayerWinState();
                    break;

            }

            if (!_deltaPause)
                _stateTimer += Time.deltaTime; //_input.deltaTime;
            else
                _deltaPause = false;
        }

        void StartGameState()
        {
            PlayerData = new PlayerInfo.PlayerData[_playerAmount];
            _wormsControllers = new WormController[_playerAmount * _wormsPerPlayer];

            float jumpHeight = GameRules.JumpHeight;
            float maxMoveSpeed = GameRules.MaxSpeed;
            float maxHealth = GameRules.WormsMaxHealth;

            /*int[] presets = {0, 1, 2, 3};

        MathHelper.ShuffleArray(ref presets);

        GameRules.playerAssignedPreset = presets;*/
        
            for (int i = 0; i < _playerAmount; i++)
            {
                PlayerData[i].Worms = new int[_wormsPerPlayer];

                for (int j = 0; j < _wormsPerPlayer; j++)
                {
                    Vector3 spawnPos;
                    bool canSpawn = false;

                    do
                    {
                        canSpawn = TryToSpawn(out spawnPos);
                    } while (!canSpawn);

                    WormController newWorm = Instantiate(playerPrefab, spawnPos, Quaternion.identity).GetComponent<WormController>();
                
                    newWorm.effects.Setup(i);

                    var transform1 = newWorm.transform;
                    newWorm.State = new PlayerInfo.WormState
                    {
                        MaxMoveSpeed = maxMoveSpeed,
                        JumpHeight = jumpHeight,
                        Transform = transform1,
                        CamFollow = transform1,
                        WormIndex = (byte)j,
                        PlayerIndex = (byte)i,
                        CurrentPlayer = false,
                        CurrentWeapon = 0,
                        CurrentWaterLevel = 0,
                        Alive = true,
                        Health = maxHealth
                    };

                    _cameraController.SetupWormCamera(ref newWorm);

                    _wormsControllers[_wormsPerPlayer * i + j] = newWorm;
                    PlayerData[i].Worms[j] = (byte)(_wormsPerPlayer * i + j);
                }
            }

            int[] weaponAmount = _wormsControllers[0].GetWeaponsAmount(true);

            for (int i = 0; i < _playerAmount; i++)
            {
                PlayerData[i].WeaponAmount = weaponAmount;
            }

            UIManager.Instance.SetUpPlayersHealth(_playerAmount, _wormsPerPlayer);

            _currentPlayer = (byte)Random.Range(0, _playerAmount);

            _currentWormController = _wormsControllers[0];
        
            _currentWormController.SetPlayerTurn(PlayerData[0]);

            SetState(LevelState.turnEnd);
        }

        private readonly Vector3 spawnRange = new Vector3(100, 100, 50);
        bool TryToSpawn(out Vector3 pos)
        {
            bool success = false;
        
            pos = new Vector3(Random.Range(-spawnRange.x, spawnRange.x), Random.Range(0, spawnRange.y),
                Random.Range(-spawnRange.z, spawnRange.z));

            RaycastHit hit;

            if (Physics.Raycast(pos + Vector3.up * .5f, Vector3.down, out hit, 100))
            {
                Vector3 normal = hit.normal;
                float steepness = Mathf.Sqrt(normal.x * normal.x + normal.z * normal.z);

                if (hit.point.y > 0 && steepness < .5f)
                    success = true;

                pos.y = hit.point.y;
            }

            return success;
        }

        void PlayerControlState()
        {
            _currentWormController.UpdateInput(_input);
        
            _cameraController.UpdateCamera(ref _currentWormController.State, ref _input);

            if (_stateTimer >= _roundTimer || _endTurn)
                SetState(LevelState.turnEnd);
        }

        [SerializeField] private Transform camTopDown;

        private byte camTransitonState;
        void EndTurnState()
        {
            if (_stateTimer == 0)
            {
                _currentWormController.CancelAction();
                PlayerData[_currentPlayer].WeaponAmount = _currentWormController.GetWeaponsAmount();

                UIManager.Instance.StartTimerUI(_roundTimer, false);
                camTransitonState = 0;
            }

            var lastPlayer = _wormsControllers[_lastWorm].State;
            var currentPlayer = _currentWormController.State;
        
            var lastCamPos = _wormsControllers[_lastWorm].transform.position + lastPlayer.CamPos;
            var targetCamPos = _currentWormController.transform.position + currentPlayer.CamPos;

            bool finished = false;
            switch (camTransitonState)
            {
                case 0:
                {
                    finished = _cameraController.TransitionCamera(lastCamPos, camTopDown.position, lastPlayer.CamRot,
                        camTopDown.eulerAngles, Time.deltaTime);
                    camTransitonState = finished ? (byte)1 : (byte)0;

                    if (finished)
                    {
                        NextPlayer(_currentPlayer);
                        _currentWormController.effects.SetHighlight(true);
                    }
                    break;
                }
                case 1:
                    if (!_currentWormController.State.Alive)
                    {
                        GetNextWorm(_currentPlayer);
                        _currentWormController.effects.SetHighlight(true);
                    }
                
                    finished = _input.AInput == 1;
                
                    camTransitonState = finished ? (byte)2 : (byte)1;
                    break;
                case 2:
                {
                    finished = _cameraController.TransitionCamera(camTopDown.position, targetCamPos, camTopDown.eulerAngles,
                        currentPlayer.CamRot, Time.deltaTime);
                    camTransitonState = finished ? (byte)3 : (byte)2;
                
                    if(finished)
                        _currentWormController.effects.SetHighlight(false);
                    break;
                }
            }

            if (camTransitonState < 3) return;
            SetState(LevelState.playerControl);
        }

        void PlayerWinState()
        {
            if (_stateTimer == 0)
            {
                UIManager.Instance.SetWinText(_currentPlayer);
                UIManager.Instance.StartTimerUI(10f, true);
            }
        
            if(_stateTimer > 10f || _input.BInput == 1)
                SceneManager.LoadScene(0);
        }

        public void ProcessDeath()
        {
            int playersAlive = _playerAmount;

            int alivePlayer = 0;

            for (int i = 0; i < _playerAmount; i++)
            {
                var playerData = PlayerData[i];
                bool isAlive = false;

                for (int j = 0; j < _wormsPerPlayer; j++)
                {
                    int worm = playerData.Worms[j];
                
                    if(!_wormsControllers[worm].State.Alive) continue;

                    alivePlayer = i;
                    isAlive = true;
                }
            
                if(isAlive) continue;

                playersAlive--;
            }
        
            if(playersAlive > 1) return;

            _currentPlayer = (byte)alivePlayer;
            SetState(LevelState.playerWin);
        }

        private bool _endTurn = false;
        public void ForceTurnEnd() => _endTurn = true;
    
        public void SetCamFollow(Transform target)
        {
            _currentWormController.State.CamFollow = target;
        }

        public void CancelCamFollow()
        {
            _currentWormController.State.CamFollow = _currentWormController.transform;
        }
    }
}

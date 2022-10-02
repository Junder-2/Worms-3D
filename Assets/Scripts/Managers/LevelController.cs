using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

    void NextPlayer()
    {
        _currentWormController.UpdateInput(new PlayerInput.InputAction());
        _currentWormController.State.currentPlayer = false;

        ProcessDeath();

        byte potentialPlayer = (byte)((_currentPlayer + 1) % _playerAmount);

        int newWorm = 0;
        bool fail = true;

        int[] wormList = PlayerData[potentialPlayer].worms;
        
        MathHelper.ShuffleArray(ref wormList);
        
        for (int i = 0; i < _wormsPerPlayer; i++)
        {
            newWorm = wormList[i];

            if (!_wormsControllers[newWorm].State.alive) continue;
            
            fail = false;
            break;
        }

        if (fail)
        {
            NextPlayer();
            return;
        }
        
        _lastWorm = _currentWorm;
        _lastPlayer = _currentPlayer;
        _currentPlayer = potentialPlayer;

        _currentWorm = (byte)newWorm;
        _currentWormController = _wormsControllers[_currentWorm];
        
        _currentWormController.State.currentPlayer = true;
        
        _currentWormController.SetPlayerTurn(PlayerData[_currentPlayer]);

        //DisplayMoveRange();
    }

    private WormController[] _wormsControllers;

    private WormController _currentWormController;

    public PlayerInfo.PlayerData[] PlayerData;

    public enum LevelState {startGame, playerControl, turnEnd, playerWin}

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

        AudioManager.Instance.PlayMusic((int)AudioSet.MusicID.Game);

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

        int[] presets = {0, 1, 2, 3};

        MathHelper.ShuffleArray(ref presets);
        
        for (int i = 0; i < _playerAmount; i++)
        {
            PlayerData[i].worms = new int[_wormsPerPlayer];

            for (int j = 0; j < _wormsPerPlayer; j++)
            {
                Vector3 spawnPos;
                bool canSpawn = false;

                do
                {
                    canSpawn = TryToSpawn(out spawnPos);
                } while (!canSpawn);

                WormController newWorm = Instantiate(playerPrefab, spawnPos, Quaternion.identity).GetComponent<WormController>();
                
                newWorm.effects.SetPresetLook(presets[i]);
                newWorm.effects.InstanceHealthUI((byte)i);

                newWorm.State = new PlayerInfo.WormState
                {
                    maxMoveSpeed = maxMoveSpeed,
                    jumpHeight = jumpHeight,
                    Transform = newWorm.transform,
                    camFollow = newWorm.transform,
                    wormIndex = (byte)j,
                    playerIndex = (byte)i,
                    currentPlayer = false,
                    currentWeapon = 0,
                    currentWaterLevel = 0,
                    alive = true,
                    health = maxHealth
                };

                _cameraController.InstantiateWormCam(ref newWorm);

                _wormsControllers[_wormsPerPlayer * i + j] = newWorm;
                PlayerData[i].worms[j] = (byte)(_wormsPerPlayer * i + j);
            }
        }

        int[] weaponAmount = _wormsControllers[0].GetWeaponsAmount(true);

        for (int i = 0; i < _playerAmount; i++)
        {
            PlayerData[i].weaponAmount = weaponAmount;
        }

        UIManager.Instance.SetPlayersHealth(_playerAmount, _wormsPerPlayer, maxHealth);

        _currentPlayer = (byte)Random.Range(0, _playerAmount);

        _currentWormController = _wormsControllers[0];

        SetState(LevelState.turnEnd);
    }

    private Vector3 spawnRange = new Vector3(100, 100, 50);
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
            PlayerData[_currentPlayer].weaponAmount = _currentWormController.GetWeaponsAmount();

            UIManager.Instance.StartTimerUI(_roundTimer, false);
            camTransitonState = 0;
        }

        var lastPlayer = _wormsControllers[_lastWorm].State;
        var currentPlayer = _currentWormController.State;
        
        var lastCamPos = _wormsControllers[_lastWorm].transform.position + lastPlayer.camPos;
        var targetCamPos = _currentWormController.transform.position + currentPlayer.camPos;

        bool finished = false;
        switch (camTransitonState)
        {
            case 0:
            {
                finished = _cameraController.TransitionCamera(lastCamPos, camTopDown.position, lastPlayer.camRot,
                    camTopDown.eulerAngles, Time.deltaTime);
                camTransitonState = finished ? (byte)1 : (byte)0;

                if (finished)
                {
                    NextPlayer();
                    _currentWormController.effects.SetHighlight(true);
                }
                break;
            }
            case 1:
                finished = _input.aInput == 1;
                
                camTransitonState = finished ? (byte)2 : (byte)1;
                break;
            case 2:
            {
                finished = _cameraController.TransitionCamera(camTopDown.position, targetCamPos, camTopDown.eulerAngles,
                    currentPlayer.camRot, Time.deltaTime);
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
        
        if(_stateTimer > 10f || _input.bInput == 1)
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
                int worm = playerData.worms[j];
                
                if(!_wormsControllers[worm].State.alive) continue;

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
        _currentWormController.State.camFollow = target;
    }

    public void CancelCamFollow()
    {
        _currentWormController.State.camFollow = _currentWormController.transform;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private byte _lastPlayer;
    private byte _currentPlayer;

    private byte _currentWorm;
    private byte _lastWorm;

    private float _maxHealth;

    void NextPlayer()
    {
        _currentWormController.UpdateInput(new PlayerInput.InputAction());
        _currentWormController.State.currentPlayer = false;
        
        _lastWorm = _currentWorm;
        _lastPlayer = _currentPlayer;
        _currentPlayer++;
        _currentPlayer %= _playerAmount;

        int newWorm = 0;
        int failCounter = 0;
        do
        {
            newWorm = Random.Range(0, _wormsPerPlayer);
            failCounter++;
        } while (!_wormsControllers[newWorm + _currentPlayer*_wormsPerPlayer].State.alive || failCounter < 8);
        

        _currentWorm = (byte)(newWorm + _currentPlayer*_wormsPerPlayer);
        _currentWormController = _wormsControllers[_currentWorm];
        
        _currentWormController.State.currentPlayer = true;
        
        _currentWormController.SetPlayerTurn(PlayerData[_currentPlayer]);

        //DisplayMoveRange();
    }

    private WormController[] _wormsControllers;

    private WormController _currentWormController;

    public PlayerInfo.PlayerData[] PlayerData;

    public enum LevelState {startGame, playerControl, turnEnd}

    private LevelState _levelState;
    private LevelState _lastLevelState;
    
    private float _stateTimer;
    private bool _deltaPause;

    public void SetState(LevelState newState)
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
        
        _playerAmount = GameRules.playerAmount;
        _wormsPerPlayer = GameRules.wormsPerPlayer;
        _roundTimer = GameRules.roundTimer;
        
        _playerInput = GetComponent<PlayerInput>();
        _cameraController = CameraController.Instance;

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

        float jumpHeight = GameRules.jumpHeight;
        float maxMoveSpeed = GameRules.maxSpeed;
        float maxDistance = GameRules.maxDistance;
        float maxHealth = GameRules.wormsMaxHealth;

        int[] presets = {0, 1, 2, 3};

        MathHelper.ShuffleArray(ref presets);
        
        for (int i = 0; i < _playerAmount; i++)
        {
            PlayerData[i].worms = new byte[_wormsPerPlayer];

            for (int j = 0; j < _wormsPerPlayer; j++)
            {
                Vector3 spawnPos;
                bool canSpawn = false;

                do
                {
                    canSpawn = TryToSpawn(out spawnPos);
                } while (!canSpawn);

                WormController newWorm = Instantiate(playerPrefab, spawnPos, Quaternion.identity).GetComponent<WormController>();
                
                newWorm.SetPresetLook(presets[i]);
                newWorm.effects.InstanceHealthUI((byte)i);

                newWorm.State = new PlayerInfo.WormState
                {
                    maxMoveSpeed = maxMoveSpeed,
                    jumpHeight = jumpHeight,
                    Transform = newWorm.transform,
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
        
        NextPlayer();
        
        SetState(LevelState.playerControl);
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

    private bool _endTurn = false;
    public void ForceTurnEnd() => _endTurn = true;
}

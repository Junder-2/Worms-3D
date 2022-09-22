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

    public float roundTimer = 30;

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

        _currentWormController.State.startPos = _currentWormController.transform.position;
        
        _currentWormController.SetPlayerTurn();

        //DisplayMoveRange();
    }

    [SerializeField] 
        private GameObject moveRangePrefab;

        private GameObject _moveRange;
    void DisplayMoveRange()
    {
        Vector3 pos = _currentWormController.State.startPos;
        
        _moveRange.SetActive(true);
        _moveRange.transform.position = pos;
        _moveRange.transform.localScale = Vector3.one*(GameRules.maxDistance*2);
    }

    private WormController[] _wormsControllers;

    private WormController _currentWormController;

    public enum LevelState {startGame, playerControl, turnEnd}

    private LevelState _levelState;
    private LevelState _lastLevelState;
    
    private float _stateTimer;
    private bool _deltaPause;

    public void SetState(LevelState newState)
    {
        if (newState == LevelState.playerControl)
        {
            UIManager.Instance.StartTimerUI(roundTimer, true);
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
        
        _playerInput = GetComponent<PlayerInput>();
        _cameraController = CameraController.Instance;
        
        _moveRange = Instantiate(moveRangePrefab, Vector3.zero, Quaternion.identity);
        _moveRange.SetActive(false);
        
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
        _wormsControllers = new WormController[_playerAmount * _wormsPerPlayer];

        float jumpHeight = GameRules.jumpHeight;
        float maxMoveSpeed = GameRules.maxSpeed;
        float maxDistance = GameRules.maxDistance;
        float maxHealth = GameRules.wormsMaxHealth;
        
        for (int i = 0; i < _playerAmount; i++)
        {
            for (int j = 0; j < _wormsPerPlayer; j++)
            {
                Vector3 spawnPos;
                bool canSpawn = false;

                do
                {
                    canSpawn = TryToSpawn(out spawnPos);
                } while (!canSpawn);

                WormController newWorm = Instantiate(playerPrefab, spawnPos, Quaternion.identity).GetComponent<WormController>();
                
                newWorm.SetPlayer(i);

                newWorm.State = new WormController.PlayerState();
                newWorm.State.maxMoveSpeed = maxMoveSpeed;
                newWorm.State.jumpHeight = jumpHeight;
                newWorm.State.Transform = newWorm.transform;
                newWorm.State.wormIndex = (byte)j;
                newWorm.State.playerIndex = (byte)i;
                //newWorm.State.maxDistance = maxDistance;
                newWorm.State.currentWeapon = 0;
                newWorm.State.currentWaterLevel = 0;
                newWorm.State.alive = true;
                newWorm.State.startPos = spawnPos;
                newWorm.State.health = maxHealth;

                _cameraController.InstantiateWormCam(ref newWorm);

                _wormsControllers[_wormsPerPlayer * i + j] = newWorm;
            }
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

        if (_stateTimer >= roundTimer || _endTurn)
            SetState(LevelState.turnEnd);
    }

    [SerializeField] private Transform camTopDown;

    private byte camTransitonState;
    void EndTurnState()
    {
        if (_stateTimer == 0)
        {
            _currentWormController.CancelAction();
            NextPlayer();
            
            UIManager.Instance.StartTimerUI(roundTimer, false);
            camTransitonState = 0;
        }

        var lastPlayer = _wormsControllers[_lastWorm].State;
        var currentPlayer = _currentWormController.State;

        switch (camTransitonState)
        {
            case 0:
            {
                bool finished = _cameraController.TransitionCamera(lastPlayer.camPos, camTopDown.position, lastPlayer.camRot,
                    camTopDown.eulerAngles, Time.deltaTime);
                camTransitonState = finished ? (byte)1 : (byte)0;
                break;
            }
            case 1:
                camTransitonState = _input.aInput == 1 ? (byte)2 : (byte)1;
                break;
            case 2:
            {
                bool finished = _cameraController.TransitionCamera(camTopDown.position, currentPlayer.camPos, camTopDown.eulerAngles,
                    currentPlayer.camRot, Time.deltaTime);
                camTransitonState = finished ? (byte)3 : (byte)2;
                break;
            }
        }

        if (camTransitonState < 3) return;
        SetState(LevelState.playerControl);
    }

    private bool _endTurn = false;
    public void ForceTurnEnd() => _endTurn = true;
}

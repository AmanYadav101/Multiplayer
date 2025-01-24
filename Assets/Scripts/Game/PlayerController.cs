using System;
using Cinemachine;
using Game.States;
using GameFramework.Network.Movement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;


public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform _camTransform;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawnPoint;
  
    //States
    private State _currentState;
    private IdleState _idleState;
    private RunState _runState;
    private AirState _airState;

    //Input
    public Input playerControl;

    [HideInInspector] public NetworkMovementComponent playerMovement;
    [HideInInspector] public new Rigidbody rigidbody;
    private float _cameraAngle;

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = _camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        cvm.Priority = IsOwner ? 1 : 0;
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        
        playerControl = new Input();
        playerMovement = GetComponent<NetworkMovementComponent>();

        _idleState = new IdleState(this);
        _runState = new RunState(this);
        _airState = new AirState(this);

        _currentState = _idleState;
        _currentState.Enter();

        playerControl.Player.Shoot.started += ctx => StartShooting();
    }

    private void OnEnable()
    {
        playerControl.Enable();
    }

    private void OnDisable()
    {
        playerControl.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        var lookInput = playerControl.Player.Look.ReadValue<Vector2>();

        if (playerMovement.IsOwner)
            playerMovement.RotatePlayerServerRPC(lookInput);
        
        
        CurrentState();
        _currentState?.Do();
    }

    void CurrentState()
    {
        Vector2 movementInput = playerControl.Player.Move.ReadValue<Vector2>();
        if (playerControl.Player.Jump.triggered && playerMovement.isGrounded)
        {
            ChangeState(_airState);
        }
        else if (movementInput != Vector2.zero)
        {
            ChangeState(_runState);
        }
        else if (movementInput == Vector2.zero)
        {
            ChangeState(_idleState);
        }
    }

    private void ChangeState(State newState)
    {
        if (_currentState == newState) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    private void StartShooting()
    {
        if (!IsOwner) return;
        ShootServerRpc();
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        GameObject projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);
        projectile.GetComponent<Rigidbody>().AddForce(_projectileSpawnPoint.right * -10f, ForceMode.Impulse);
        Destroy(projectile, 5);
    }
}
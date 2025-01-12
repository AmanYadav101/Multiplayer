using Cinemachine;
using Game.States;
using GameFramework.Network.Movement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    // [SerializeField] private float _speed;
    // [SerializeField] private float _turnSpeed;
    // [SerializeField] private Vector2 _minMaxRotationX;

    [SerializeField] private Transform _camTransform;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawnPoint;
    public CharacterController _characterController;

    //States
    private State _currentState;
    private IdleState _idleState;
    private RunState _runState;
    private AirState _airState;

    public Input playerControl;
    public NetworkMovementComponent playerMovement;
    private float _cameraAngle;

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = _camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        cvm.Priority = IsOwner ? 1 : 0;
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
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
     bool isGrounded = _characterController.isGrounded;
     if (!isGrounded)
     {
         _characterController.Move(new Vector3(0.0f, -9.8f * Time.deltaTime, 0.0f));   
     }
        Vector2 lookInput = playerControl.Player.Look.ReadValue<Vector2>();
        playerMovement.RotatePlayer(lookInput);
        CurrentState();
        _currentState?.Do();
    }

    void CurrentState()
    {
        Vector2 movementInput = playerControl.Player.Move.ReadValue<Vector2>();
        var jumpInput = playerControl.Player.Jump.ReadValue<float>();
        if (movementInput != Vector2.zero)
        {
            ChangeState(_runState);
        }
        else if (movementInput == Vector2.zero)
        {
            ChangeState(_idleState);
        }


        if (jumpInput != 0)
        {
            ChangeState(_airState);
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
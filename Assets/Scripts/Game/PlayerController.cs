using System;
using System.Collections;
using Cinemachine;
using GameFramework.Network.Movement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    // [SerializeField] private float _speed;
    // [SerializeField] private float _turnSpeed;
    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _camTransform;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawnPoint;
    private NetworkMovementComponent _playerMovement;
    private CharacterController _cc;

    private Input _playerControl;
    private float _cameraAngle;
    private ulong clientId;

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = _camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        cvm.Priority = IsOwner ? 1 : 0;
    }

    private void Awake()
    {
        _playerMovement = GetComponent<NetworkMovementComponent>();
        _playerControl = new Input();

        _playerControl.Player.Shoot.started += ctx => StartShooting();
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


    private void OnEnable()
    {
        _playerControl.Enable();
    }

    private void OnDisable()
    {
        _playerControl.Disable();
    }

    void Start()
    {
        _playerMovement = GetComponent<NetworkMovementComponent>();
        if (_playerMovement == null)
        {
            Debug.LogError("NetworkMovementComponent is not attached to the GameObject.");
        }

        _cc = GetComponent<CharacterController>();
        if (_cc == null)
        {
            Debug.LogError("CharacterController is not attached to the GameObject.");
        }


        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 movementInput = _playerControl.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();


        if (_playerMovement)
        {
            if (IsClient && IsLocalPlayer)
            {
                _playerMovement.ProcessLocalPlayerMovement(movementInput, lookInput);
            }
            else
            {
                _playerMovement.ProcessSimulatedPlayerMovement();
            }
        }
        else
        {
            Debug.LogError("NetworkMovementComponent is null, skipping movement processing.");
        }
    }
}
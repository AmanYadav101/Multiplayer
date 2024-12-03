using System;
using Cinemachine;
using GameFramework.Network.Movement;
using Unity.Netcode;

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    // [SerializeField] private float _speed;
    // [SerializeField] private float _turnSpeed;
    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _camTransform;
    private NetworkMovementComponent _playerMovement;
    private CharacterController _cc;

    private Input _playerControl;
    private float _cameraAngle;

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = _camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        cvm.Priority = IsOwner ? 1 : 0;
    }

    private void Awake()
    {
        _playerMovement = GetComponent<NetworkMovementComponent>();

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

        _playerControl = new Input();
        _playerControl.Enable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 movementInput = _playerControl.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();

        if (_playerMovement != null)
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
// private void Move(Vector2 movementInput)
    // {
    //     Vector3 movement = movementInput.x * _camTransform.right + movementInput.y * _camTransform.forward;
    //
    //     movement.y = 0;
    //
    //     _cc.Move(movement * (_speed * Time.deltaTime));
    // }
    //
    // private void Rotate(Vector2 lookInput)
    // {
    //     transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * Time.deltaTime);
    // }

    // private void RotateCamera(Vector2 lookInput)
    // {
    //     
    //     float mouseX = lookInput.x;
    //     float mouseY = lookInput.y;
    //     transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * _turnSpeed));
    //
    //     _xRotation -= (mouseY * Time.deltaTime) * _turnSpeed;
    //     _xRotation = Mathf.Clamp(_xRotation, -45f, 40f);
    //     _camTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    //     
    //     // _cameraAngle = Vector3.SignedAngle(transform.forward, _camTransform.forward, _camTransform.right);
    //     // float cameraRotationMovement = lookInput.y * _turnSpeed * Time.deltaTime;
    //     // float newCameraAngle = _cameraAngle - cameraRotationMovement;
    //     // if (newCameraAngle <= _minMaxRotationX.x && newCameraAngle >= _minMaxRotationX.y)
    //     // {
    //     //     Debug.Log("Rotating Camera ");
    //     //     _camTransform.RotateAround(_camTransform.position, _camTransform.right,
    //     //         -lookInput.y * _turnSpeed * Time.deltaTime);
    //  //   }
    // }

    // [ServerRpc]
    // private void MoveServerRPC(Vector2 movementInput, Vector2 lookInput)
    // {
    //     Move(movementInput);
    //     Rotate(lookInput);
    // }

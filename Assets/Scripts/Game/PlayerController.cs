using System;
using Cinemachine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _camTransform;
    private float _xRotation = 0f;

    private CharacterController _cc;

    private Input _playerControl;
    private float _cameraAngle;


    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = _camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        cvm.Priority = IsOwner ? 1 : 0;
    }

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _playerControl = new Input();
        _playerControl.Enable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 movementInput = _playerControl.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();

        if (IsServer && IsLocalPlayer)
        {
            Move(movementInput);
            Rotate(lookInput);
            RotateCamera(lookInput);
            
        }
        else if (IsLocalPlayer)
        {
            MoveServerRPC(movementInput, lookInput);
            RotateCamera(lookInput);
        }
    }

    private void Move(Vector2 movementInput)
    {
        Vector3 movement = movementInput.x * _camTransform.right + movementInput.y * _camTransform.forward;

        movement.y = 0;

        _cc.Move(movement * (_speed * Time.deltaTime));
    }

    private void Rotate(Vector2 lookInput)
    {
        transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * Time.deltaTime);
    }

    private void RotateCamera(Vector2 lookInput)
    {
        
        float mouseX = lookInput.x;
        float mouseY = lookInput.y;
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * _turnSpeed));

        _xRotation -= (mouseY * Time.deltaTime) * _turnSpeed;
        _xRotation = Mathf.Clamp(_xRotation, -45f, 40f);
        _camTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        
        // _cameraAngle = Vector3.SignedAngle(transform.forward, _camTransform.forward, _camTransform.right);
        // float cameraRotationMovement = lookInput.y * _turnSpeed * Time.deltaTime;
        // float newCameraAngle = _cameraAngle - cameraRotationMovement;
        // if (newCameraAngle <= _minMaxRotationX.x && newCameraAngle >= _minMaxRotationX.y)
        // {
        //     Debug.Log("Rotating Camera ");
        //     _camTransform.RotateAround(_camTransform.position, _camTransform.right,
        //         -lookInput.y * _turnSpeed * Time.deltaTime);
     //   }
    }

    [ServerRpc]
    private void MoveServerRPC(Vector2 movementInput, Vector2 lookInput)
    {
        Move(movementInput);
        Rotate(lookInput);
    }
}
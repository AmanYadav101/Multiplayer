using System;
using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Network.Movement
{
    public class NetworkMovementComponent : NetworkBehaviour
    {
        [SerializeField] private CharacterController _cc;
        [SerializeField] private float _speed;
        [SerializeField] private float _turnSpeed;

        [SerializeField] private Transform _camSocket;
        [SerializeField] private GameObject _vcam;

        private Transform _vcamTransform;

        private int _tick = 0;
        private float _tickRate = 1f / 144f;
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        public TransformState _previousTransformState;


        private void OnEnable()
        {
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _vcamTransform = _vcam.transform;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            _previousTransformState = previousvalue;
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput)
        {
            Debug.Log("ProcessLocalPlayerMovement");

            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                int bufferIndex = _tick % BUFFER_SIZE;
                if (!IsServer)
                {
                    MovePlayerServerRPC(_tick, movementInput, lookInput);
                    MovePlayer(movementInput);
                    RotatePlayer(lookInput);
                }
                else
                {
                    MovePlayer(movementInput);
                    RotatePlayer(lookInput);

                    TransformState state = new TransformState()
                    {
                        Tick = _tick,
                        Position = transform.position,
                        Rotation = transform.rotation,
                        HasStartedMoving = true
                    };
                    _previousTransformState = ServerTransformState.Value;
                    ServerTransformState.Value = state;
                }

                InputState inputState = new InputState()
                {
                    Tick = _tick,
                    MovementInput = movementInput,
                    LookInput = lookInput
                };
                TransformState transformState = new TransformState()
                {
                    Tick = _tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };

                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }


        public void ProcessSimulatedPlayerMovement()
        {
            Debug.Log("ProcessSimulatedPlayerMovement");
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    transform.position = ServerTransformState.Value.Position;
                    transform.rotation = ServerTransformState.Value.Rotation;
                }
        
                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }

        
        // private Vector3 extrapolatedPosition;
        // private Quaternion extrapolatedRotation;
        //
        // public void ProcessSimulatedPlayerMovement()
        // {
        //     _tickDeltaTime += Time.deltaTime;
        //
        //     if (ServerTransformState.Value.HasStartedMoving)
        //     {
        //         // Predict the next position based on the velocity and direction
        //         Vector3 movementVector = ServerTransformState.Value.Position - _previousTransformState.Position;
        //         extrapolatedPosition = ServerTransformState.Value.Position + movementVector;
        //
        //         // Predict the next rotation
        //         extrapolatedRotation = ServerTransformState.Value.Rotation;
        //
        //         // Smooth transition to the predicted values
        //         transform.position = Vector3.Lerp(
        //             transform.position,
        //             extrapolatedPosition,
        //             Time.deltaTime * 5f // Adjust extrapolation speed
        //         );
        //
        //         transform.rotation = Quaternion.Slerp(
        //             transform.rotation,
        //             extrapolatedRotation,
        //             Time.deltaTime * 5f // Adjust extrapolation speed
        //         );
        //     }
        //
        //     _tickDeltaTime -= _tickRate;
        //     _tick++;
        // }


        private void MovePlayer(Vector2 movementInput)
        {
            if (_cc == null)
            {
                Debug.LogError("_cc (CharacterController) is null in MovePlayer.");
                return;
            }

            if (_vcamTransform == null)
            {
                Debug.LogError("_vcamTransform is null in MovePlayer.");
                return;
            }


            Vector3 movement = movementInput.x * _vcamTransform.right + movementInput.y * _vcamTransform.forward;
            movement.y = 0; // Flatten movement on the Y axis
            if (!_cc.isGrounded)
            {
                movement.y = -9.61f; // Simulate gravity
            }

            _cc.Move(movement * _speed * _tickRate);
        }


        private void RotatePlayer(Vector2 lookInput)
        {
            _vcamTransform.RotateAround(_vcamTransform.position, _vcamTransform.right,
                -lookInput.y * _turnSpeed * _tickRate);
            transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * _tickRate);
        }

        [ServerRpc]
        private void MovePlayerServerRPC(int tick, Vector2 movementInput, Vector2 lookInput)
        {
            MovePlayer(movementInput);
            RotatePlayer(lookInput);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };


            _previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;
        }
    }
}
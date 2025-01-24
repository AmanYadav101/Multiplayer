using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameFramework.Network.Movement
{
    public class NetworkMovementComponent : NetworkBehaviour
    {
        // [SerializeField] private CharacterController _cc;
        [SerializeField] private float _speed;
        [SerializeField] private float _turnSpeed;
        private Vector2 _accumulatedRotation;
        [SerializeField] private Transform _camSocket;
        [SerializeField] private GameObject _vcam;
        Rigidbody _rigidbody;
        private Transform _vCamTransform;
        private Vector3 _targetDirection;
        private int _tick = 0;
        private float _tickRate = 1f / 60;
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private readonly InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private readonly TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> serverTransformState = new NetworkVariable<TransformState>();
        private TransformState _previousTransformState;


        //Grounded
        [HideInInspector] public bool isGrounded;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float rayLength = 1;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            serverTransformState.OnValueChanged += OnServerStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _vCamTransform = _vcam.transform;
        }


        private void OnServerStateChanged(TransformState previousValue, TransformState newValue)
        {
            _previousTransformState = previousValue;
        }

        private void Update()
        {
            isGrounded = Physics.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput)
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                int bufferIndex = _tick % BUFFER_SIZE;

                MovePlayerServerRPC(_tick, movementInput, lookInput);
                MovePlayer(movementInput);

                InputState inputState = new InputState()
                {
                    Tick = _tick,
                    MovementInput = movementInput,
                    // LookInput = lookInput
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
                if (_tick == BUFFER_SIZE)
                {
                    _tick = 0;
                }
                else
                    _tick++;
            }
        }


        public void ProcessSimulatedPlayerMovement()
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                if (serverTransformState.Value.HasStartedMoving)
                {
                    transform.position = serverTransformState.Value.Position;
                    transform.rotation = serverTransformState.Value.Rotation;
                }

                _tickDeltaTime -= _tickRate;
                if (_tick == BUFFER_SIZE)
                {
                    _tick = 0;
                }
                else
                {
                    _tick++;
                }
            }
        }


        private void MovePlayer(Vector2 movementInput)
        {
            Vector3 dir = new Vector3(movementInput.x, 0, movementInput.y);

            Vector3 camDirection = _vCamTransform.rotation * dir;
            _targetDirection = new Vector3(camDirection.x, 0, camDirection.z);
            Vector3 currentVelocity = _rigidbody.velocity;

            Vector3 newVelocity = _targetDirection.normalized * (_speed);
            newVelocity.y = currentVelocity.y;
            _rigidbody.velocity = newVelocity;
        }


        private void RotatePlayer(Vector2 lookInput)
        {
            transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * _tickRate);

            _vCamTransform.RotateAround(_vCamTransform.position, _vCamTransform.right,
                -lookInput.y * _turnSpeed * _tickRate);
        }

        [Rpc(SendTo.Server)]
        private void MovePlayerServerRPC(int tick, Vector2 movementInput, Vector2 lookInput)
        {
            MovePlayer(movementInput);


            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };


            _previousTransformState = serverTransformState.Value;
            serverTransformState.Value = state;
        }

        [Rpc(SendTo.Server)]
        public void RotatePlayerServerRPC(Vector2 lookInput)
        {
            RotatePlayer(lookInput);
        }
    }
}
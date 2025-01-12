using UnityEngine;

namespace Game.States
{
    public class RunState : State
    {
        public RunState(PlayerController player)
        {
            _player = player;
        }
        public override void Enter()
        {
            Debug.Log("Entering Run State");
        }

        public override void Do()
        {
            Vector2 movementInput = _player.playerControl.Player.Move.ReadValue<Vector2>();
            Vector2 lookInput = _player.playerControl.Player.Look.ReadValue<Vector2>();
            if (_player.playerMovement)
            {
                if (_player.IsClient && _player.IsLocalPlayer)
                {
                    _player.playerMovement.ProcessLocalPlayerMovement(movementInput, lookInput);
                }
                else
                {
                    _player.playerMovement.ProcessSimulatedPlayerMovement();
                }
            }
        }

        public override void FixedDo()
        {
        }

        public override void Exit()
        {
            Debug.Log("Exiting Run State");
        }
    }
}
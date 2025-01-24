using UnityEngine;

namespace Game.States
{
    public class AirState : RunState
    {
        private float _jumpForce = 5f;
        public AirState(PlayerController player) : base(player)
        {
            _player = player;
        }

        public override void Enter()
        {
            Debug.Log("Entering Air State");
            
        }

        public override void Do()
        {
            base.Do();
            _player.rigidbody.AddForce(_player.gameObject.transform.up*_jumpForce, ForceMode.Impulse);
        }

        public override void FixedDo()
        {
        }

        public override void Exit()
        {
            Debug.Log("Exiting Air State");
        }
    }
}
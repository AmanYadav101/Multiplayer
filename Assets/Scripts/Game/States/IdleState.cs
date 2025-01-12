using UnityEngine;

namespace Game.States
{
    public class IdleState : State
    {
        public IdleState(PlayerController player)
        {
            _player = player;
        }
        public override void Enter()
        {
            Debug.Log("Entering Idle State");
        }

        public override void Do()
        {
        }

        public override void FixedDo()
        {
        }

        public override void Exit()
        {
            Debug.Log("Exiting Idle State");

        }
    }
}
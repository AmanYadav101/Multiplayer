using UnityEngine;

namespace Game.States
{
    public class AirState : State
    {
        public Vector3 jump;
        public float jumpForce = 20f;
        public AirState(PlayerController player)
        {
            _player = player;
            jump = new Vector3(0.0f,1.0f, 0.0f);
        }

        public override void Enter()
        {
            Debug.Log("Entering Air State");
            
        }

        public override void Do()
        {
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
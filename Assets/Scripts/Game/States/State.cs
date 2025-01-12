using GameFramework.Network.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Game.States
{
    public abstract class State 
    {
        public bool isComplete{get; protected set;}
        protected float startTime;
        public float time => Time.time - startTime;
        protected PlayerController _player;

        public virtual void Enter(){}
        public virtual void Do(){}
        public virtual void FixedDo(){}
        public virtual void Exit(){}
        
     
    }
}

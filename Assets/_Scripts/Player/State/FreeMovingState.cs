using UnityEngine;

namespace _Scripts.Player.State
{
    public class FreeMovingState : IPlayerState
    {
        public void EnterState() {}

        public void UpdateState()
        {
            PlayerMovement.Instance.GatherInput();
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.CheckCollisions();
            
            PlayerMovement.Instance.HandleJump();
            PlayerMovement.Instance.HandleDirection();
            PlayerMovement.Instance.HandleGravity();

            PlayerMovement.Instance.ApplyMovement();
        }
        public void ExitState() {}
    }
}
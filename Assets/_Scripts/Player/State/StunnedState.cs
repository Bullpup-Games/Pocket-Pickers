using UnityEngine;

namespace _Scripts.Player.State
{
    public class StunnedState : IPlayerState
    {
        public void EnterState()
        {
            PlayerMovement.Instance.HaltHorizontalMomentum();
        }

        public void UpdateState()
        {
            // PlayerMovement.Instance.AlterHorizontalMovement(0.95f);
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.CheckCollisions();
            
            PlayerMovement.Instance.HandleJump();
            PlayerMovement.Instance.HandleDirection();
            PlayerMovement.Instance.HandleGravity();

            PlayerMovement.Instance.ApplyMovement();
        }
        public void ExitState()
        {
        }
    }
}
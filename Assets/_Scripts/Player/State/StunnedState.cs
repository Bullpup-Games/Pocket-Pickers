using UnityEngine;

namespace _Scripts.Player.State
{
    public class StunnedState : IPlayerState
    {
        public void EnterState()
        {
            PlayerMovement.Instance.HaltHorizontalMomentum();
            PlayerMovement.Instance.ResetFrameInput();
        }

        public void UpdateState()
        {
            PlayerMovement.Instance.AlterHorizontalMovement(0.95f);
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.CheckCollisions();
            PlayerMovement.Instance.HandleGravity();

            PlayerMovement.Instance.ApplyMovement();
        }
        public void ExitState()
        {
        }
    }
}
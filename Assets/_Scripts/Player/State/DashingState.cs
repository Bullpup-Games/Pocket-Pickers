using System.Collections;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class DashingState : IPlayerState
    {
        private Coroutine _dashCoroutine;
        private Vector2 _dashDirection;
        public void EnterState()
        {
            // Ensure we have the latest input
            PlayerMovement.Instance.GatherInput();

            var inputDirection = PlayerMovement.Instance.FrameInput;

            // If no input is given or it is too small to recognize default to the facing direction
            /*if (Mathf.Abs(inputDirection.x) < 0.1f && Mathf.Abs(inputDirection.y) < 0.1f)
            {
                inputDirection = PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;
            }
            
            _dashDirection = inputDirection;*/

            // PlayerMovement.Instance.DashDirection = _dashDirection;
            PlayerMovement.Instance.DashDirection = 
                PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;

            _dashCoroutine = PlayerStateManager.Instance.StartCoroutine(DashCoroutine());
        }

        public void UpdateState()
        {
            
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.CheckCollisions();
            // PlayerMovement.Instance.HandleGravity();
            PlayerMovement.Instance.ApplyDashMovement();
        }

        public void ExitState()
        {
            if (_dashCoroutine == null) return;
            PlayerStateManager.Instance.StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }

        private IEnumerator DashCoroutine()
        {
            yield return new WaitForSeconds(PlayerVariables.Instance.Stats.DashDuration);
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }
    }
}
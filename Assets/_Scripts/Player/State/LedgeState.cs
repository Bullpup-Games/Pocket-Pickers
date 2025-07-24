using System.Collections;
using _Scripts.Card;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class LedgeState : IPlayerState
    {
        private Coroutine _slideToLedgePosCoroutine;
        public void EnterState()
        {
            PlayerMovement.Instance.HaltHorizontalMomentum();
            
            PlayerMovement.Instance.HaltVerticalMomentum();

            CardManager.Instance.Teleport += teleported;
            
            _slideToLedgePosCoroutine = PlayerStateManager.Instance.StartCoroutine(LerpToLedgeHangPosition());
            
            PlayerAnimator.Instance.ledgeHang();
        }

        public void UpdateState()
        {
            PlayerMovement.Instance.GatherInput();
            
            if (PlayerMovement.Instance.JumpDownFrameInput)
            {
                PlayerMovement.Instance.HandleWallJump();
                PlayerAnimator.Instance.endHang();
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
                return;
            } 
            
            if (_slideToLedgePosCoroutine is null)
                PlayerMovement.Instance.HaltVerticalMomentum();
            
            PlayerMovement.Instance.HandleWallJump();

            CheckInputIsPresent();

            // If somehow the player has slid enough to where the ledge check cast is hitting a wall transition to wall state
            if (!PlayerMovement.Instance.IsLedged())
            {
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.WallState);
            }

            // Check for downward input on the left stick, this should transition the player smoothly to Wall State
            if (PlayerMovement.Instance.FrameInput.y < -0.5f)
            {
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.WallState);
            }
            
            PlayerMovement.Instance.PushPlayerTowardsWall();
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.ApplyMovement();
        }
        public void ExitState()
        {
            PlayerAnimator.Instance.endHang();
            PlayerStateManager.Instance.setLastLedgeHangTime(Time.time);
            
            if (_slideToLedgePosCoroutine is null) return;
            PlayerStateManager.Instance.StopCoroutine(_slideToLedgePosCoroutine);
            _slideToLedgePosCoroutine = null;
        }
        
        
        private void CheckInputIsPresent()
        {
            if (PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x > 0) return;
            if (!PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x < 0) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }

        public void teleported(Vector2 catchEvent)
        {
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }
        
        private IEnumerator LerpToLedgeHangPosition()
        {
            // PlayerAnimator.Instance.ledgeHang();
            while (true)
            {
                var rayOrigin = (Vector2)PlayerVariables.Instance.Collider2D.bounds.center +
                                Vector2.up * (PlayerVariables.Instance.Collider2D.bounds.extents.y - 0.7f);

                var direction = PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;

                var hit = Physics2D.Raycast(rayOrigin, direction, 0.75f, PlayerMovement.Instance.wallLayer);

                if (hit.collider != null)
                {
                    // The ray hit the wall; exit the loop
                    break;
                }

                // Break the coroutine on downward joystick input
                if (PlayerMovement.Instance.FrameInput.y < 0)
                {
                    yield break;
                }

                if (PlayerMovement.Instance.IsGrounded() || !PlayerMovement.Instance.IsWalled() || !PlayerMovement.Instance.IsLedged())
                {
                    yield break;
                }

                PlayerMovement.Instance.WallSlideMovement();
                
                yield return null;
            }
            
            // After exiting the loop ensure vertical momentum is halted
            PlayerMovement.Instance.HaltVerticalMomentum();
            _slideToLedgePosCoroutine = null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class WallState : IPlayerState
    {
        private Coroutine _slideToLedgePosCoroutine;
        public void EnterState()
        {
            PlayerMovement.Instance.LerpVerticalMomentum();
        }

        public void UpdateState()
        {
            PlayerMovement.Instance.GatherInput();

            if (PlayerMovement.Instance.JumpDownFrameInput)
            {
                PlayerMovement.Instance.HandleWallJump();
                PlayerAnimator.Instance.endSlide();
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
                return;
            }

            PlayerAnimator.Instance.wallSlide();
            PlayerMovement.Instance.WallSlideMovement();
            PlayerMovement.Instance.PushPlayerTowardsWall();
            
            PlayerMovement.Instance.HandleWallJump();
            CheckWallAndGroundConditions();
            CheckInputIsPresent();
           
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.ApplyMovement();
        }

        public void ExitState()
        {
            if (_slideToLedgePosCoroutine != null)
            {
                PlayerStateManager.Instance.StopCoroutine(_slideToLedgePosCoroutine);
                _slideToLedgePosCoroutine = null;
            }

            // Set the last wall hang time to prevent immediate re-entry into WallState
            PlayerStateManager.Instance.SetLastWallHangTime(Time.time);
            PlayerAnimator.Instance.endSlide();
        }

        /*
         * To stay on a wall the player should be pushing their left stick in the direction of the wall.
         * When they release it they should exit wall state and begin a free fall.
         */
        private static void CheckInputIsPresent()
        {
            if (PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x > 0) return;
            if (!PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x < 0) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }

        private static void CheckWallAndGroundConditions()
        {
            if (!PlayerMovement.Instance.IsGrounded() && PlayerMovement.Instance.IsWalled()) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }
    }
}
using Assets._Scripts.Context;
using Assets._Scripts.Input;
using UnityEngine;

namespace Assets._Scripts.Movement
{
    public class PlayerMovement : BaseMovement
    {
        private readonly PlayerContext context;

        private Vector3 frameVelocity;
        private FrameInput frameInput;
        private readonly float time;

        // Collision / jump state
        private bool grounded;
        private bool jumpToConsume;
        private bool coyoteUsable;
        private bool bufferedJumpUsable;
        private bool endedJumpEarly;
        private float frameLeftGrounded;
        private float timeJumpWasPressed;

        /// <summary>
        /// Constructs movement logic with access to all required contextual data
        /// (Rigidbody, stats, input, etc.)
        /// </summary>
        public PlayerMovement(PlayerContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gathers raw frame input from the InputHandler.
        /// This data drives the next Tick cycle (currently only movement and jump).
        /// </summary>
        /// <param name="jumpPressed">True if jump was pressed this frame</param>
        /// <param name="jumpHeld">True if jump is still being held</param>
        /// <param name="moveInput"> Horizontal movement input</param>
        public void GatherInput(bool jumpPressed, bool jumpHeld, Vector2 moveInput)
        {
            frameInput = new FrameInput
            {
                JumpDown = jumpPressed,
                JumpHeld = jumpHeld,
                Move = moveInput
            };

            if (frameInput.JumpDown)
            {
                jumpToConsume = true;
                timeJumpWasPressed = time;
            }

            if (frameInput.Move.x != 0)
            {
                // Flip sprite direction based on moveInput
            }
        }

        // Example of State's potential update loop
        // public void Update()
        // {
        //     time += Time.deltaTime;

        //     // Read current Rigidbody velocity from context
        //     frameVelocity = context.RigidBody.velocity;

        //     CheckCollisions();
        //     Jump();
        //     HandleDirection(Time.deltaTime);
        //     HandleGravity();

        //     // Ensure velocity is clamped to 2D
        //     frameVelocity.z = 0f;

        //     // Apply velocity back to context
        //     context.RigidBody.velocity = frameVelocity;
        // }

        /// <summary>
        /// Uses raycasts to determine grounded and ceiling state, triggering jump logic flags.
        /// Based on result, enables coyote and buffered jump timers.
        /// </summary>
        private void CheckCollisions()
        {
            bool groundHit = context.IsGrounded(context.Stats.GrounderDistance);
            bool ceilingHit = context.IsTouchingCeiling(context.Stats.GrounderDistance);

            if (ceilingHit)
                frameVelocity.y = Mathf.Min(0, frameVelocity.y);

            if (!grounded && groundHit)
            {
                grounded = true;
                coyoteUsable = true;
                bufferedJumpUsable = true;
                endedJumpEarly = false;
            }
            else if (grounded && !groundHit)
            {
                grounded = false;
                frameLeftGrounded = time;
            }
        }


        /// <summary>
        /// Handles jump decision based on coyote time and buffered jump input.
        /// If allowed, triggers the jump execution.
        /// </summary>
        public override void Jump()
        {
            bool canUseCoyote = coyoteUsable && !grounded && time < frameLeftGrounded + context.Stats.CoyoteTime;
            bool hasBufferedJump = bufferedJumpUsable && time < timeJumpWasPressed + context.Stats.JumpBuffer;

            if (!jumpToConsume && !hasBufferedJump) return;

            if (grounded || canUseCoyote)
            {
                ExecuteJump();
            }

            jumpToConsume = false;
        }

        /// <summary>
        /// Sets vertical velocity based on current jump power.
        /// Disables coyote/buffered jump and resets relevant flags.
        /// </summary>
        private void ExecuteJump()
        {
            endedJumpEarly = false;
            bufferedJumpUsable = false;
            coyoteUsable = false;

            frameVelocity.y = context.Stats.JumpPower;
        }

        /// <summary>
        /// Smoothly adjusts horizontal velocity toward input target speed using air or ground acceleration.
        /// </summary>
        /// <param name="deltaTime">Fixed timestep</param>
        private void HandleDirection(float deltaTime)
        {
            float targetSpeed = frameInput.Move.x * context.Stats.MaxSpeed;
            float accel = grounded ? context.Stats.Acceleration : context.Stats.AirAcceleration;

            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, targetSpeed, accel * deltaTime);
        }

        /// <summary>
        /// Applies vertical acceleration based on grounded state and gravity modifiers (early jump release, max fall speed).
        /// </summary>
        public override void HandleGravity()
        {
            if (grounded && frameVelocity.y <= 0f)
            {
                frameVelocity.y = context.Stats.GroundingForce;
            }
            else
            {
                float gravity = context.Stats.FallAcceleration;

                if (endedJumpEarly && frameVelocity.y > 0)
                    gravity *= context.Stats.JumpEndEarlyGravityModifier;

                frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -context.Stats.MaxFallSpeed, gravity * Time.deltaTime);
            }
        }
    }
}
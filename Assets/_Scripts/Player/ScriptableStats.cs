using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu]
    public class ScriptableStats : ScriptableObject
    {
        [Header("LAYERS")] [Tooltip("Set this to the layer your player is on")]
        public LayerMask PlayerLayer;

        [Header("INPUT")] [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
        public bool SnapInput = true;

        [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;

        [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;

        [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
        public float MaxSpeed = 14;

        [Tooltip("The player's capacity to gain horizontal speed")]
        public float Acceleration = 120;

        [Tooltip("The pace at which the player comes to a stop")]
        public float GroundDeceleration = 60;

        [Tooltip("Deceleration in air only after stopping input mid-air")]
        public float AirDeceleration = 30;

        [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
        public float GroundingForce = -1.5f;

        [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
        public float GrounderDistance = 0.05f;

        [Header("JUMP")] [Tooltip("The immediate velocity applied when jumping")]
        public float JumpPower = 36;

        [Tooltip("The maximum vertical movement speed")]
        public float MaxFallSpeed = 40;

        [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
        public float FallAcceleration = 110;

        [Tooltip("The gravity multiplier added when jump is released early")]
        public float JumpEndEarlyGravityModifier = 3;

        [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
        public float CoyoteTime = .15f;

        [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
        public float JumpBuffer = .2f;

        [Header("DASH")] 
        [Tooltip("The speed the player will travel during their dash")]
        public float DashSpeed = 22f;

        [Tooltip("The amount of time a dash will last in seconds")]
        public float DashDuration = 0.15f;

        [Tooltip("The cooldown between available dashes in seconds")]
        public float DashCooldown = 1.25f;

        [Header("WALL")] 
        [Tooltip("Falling or 'Sliding' speed when holding onto a wall")]
        public float WallSlideSpeed = 8f;

        [Tooltip("The amount of time in seconds required in between seperate wall hangs")]
        public float WallHangCooldown = 0.525f;
        
        [Tooltip ("The amount of time in seconds required in between seperate ledge hangs")]
        public float LedgeHangCooldown = 0.525f;
        
        [Tooltip("Unique coyote time for leaving wall hangs")]
        public float WallCoyoteTime = .25f;
        
        [Tooltip("Jump power when wall hanging")]
        public float WallJumpPower = 18f;

        [Header("CROUCH")] 
        [Tooltip("The maximum horizontal speed which crouching")]
        public float MaxCrouchSpeed = 6f;
        
        [Tooltip("Jump power when crouching")]
        public float CrouchJumpPower = 21f;
        
        [Header("TELPORT")] 
        [Tooltip(
            "The amount of time in seconds that the player will spend gradually transitioning to the max fall speed instead of hitting it straight await." +
            "This is just meant to help players track the character better after teleporting in the air, and makes the teleport feel more forgiving")]
        public float TeleportHangTime = 0.35f;
        
        [Tooltip("The amount of times the player is allowed to throw the card while in the air before needing to touch the ground. " +
                 "If the limit is reached the player will not be able to throw a card again until they are grounded")]
        public int AirTimeCardThrowLimit = 2;

    }
}
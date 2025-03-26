using UnityEngine;

namespace Assets._Scripts.Context
{
    public class EntityContext
    {
        public Rigidbody RigidBody;
        public Collider Collider;
        public Transform Transform;
        public LayerMask GroundMask { get; private set; }
        public LayerMask CeilingMask { get; private set; }
        public LayerMask WallMask { get; private set; }

        public int Health;
        public float Gravity;

        /// <summary>
        /// EntityContext is a util to store valuable information that is needed in multiple places.
        /// Does not derive from MonoBehavior so can be constructed by Entities and their children.
        /// </summary>
        public EntityContext(Rigidbody rb, Collider col, Transform transform, int health, float gravity)
        {
            RigidBody = rb;
            Collider = col;
            Transform = transform;
            Health = health;
            Gravity = gravity;

            GroundMask = LayerMask.NameToLayer("Ground");
            CeilingMask = LayerMask.NameToLayer("Ceiling");
            WallMask = LayerMask.NameToLayer("Wall");
        }

        /// <summary>
        /// Checks if the entity is grounded using a downward raycast.
        /// </summary>
        public bool IsGrounded(float groundCheckDistance)
        {
            Vector3 origin = Collider.bounds.center;
            float castDistance = groundCheckDistance + 0.1f;

            return Physics.Raycast(origin, Vector3.down, castDistance, GroundMask);
        }

        /// <summary>
        /// Checks if the entity is hitting a ceiling using an upward raycast.
        /// </summary>
        public bool IsTouchingCeiling(float ceilingCheckDistance)
        {
            Vector3 origin = Collider.bounds.center;
            float castDistance = ceilingCheckDistance + 0.1f;

            return Physics.Raycast(origin, Vector3.up, castDistance, CeilingMask);
        }

        /// <summary>
        /// Checks for walls on the left/right side
        /// </summary>
        public bool IsTouchingWall(Vector3 direction, float wallCheckDistance)
        {
            Vector3 origin = Collider.bounds.center;
            Vector3 dir = new Vector3(Mathf.Sign(direction.x), 0f, 0f); // Only check X-direction

            return Physics.Raycast(origin, dir, wallCheckDistance, WallMask);
        }

#if UNITY_EDITOR
        public void DebugDrawGizmos(float checkDistance, LayerMask mask)
        {
            // Optional: visualize in editor
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Collider.bounds.center, Collider.bounds.center + Vector3.down * checkDistance);
        }
#endif
    }
}
using UnityEngine;

namespace _Scripts.Card
{
    public class CardRotationController : MonoBehaviour
    {
        private Vector2 _previousPosition;

        private void Start()
        {
            _previousPosition = transform.position;

            var rb = GetComponent<Rigidbody2D>();
            if (rb is null) return;
            
            var initialVelocity = rb.velocity;
            if (!(initialVelocity.sqrMagnitude > 0.0001f)) return;
            
            var angle = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void FixedUpdate()
        {
            Vector2 currentPosition = transform.position;
            var movementDirection = currentPosition - _previousPosition;

            if (movementDirection.sqrMagnitude > 0.0001f)
            {
                var angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            _previousPosition = currentPosition;
        } 
    }
}
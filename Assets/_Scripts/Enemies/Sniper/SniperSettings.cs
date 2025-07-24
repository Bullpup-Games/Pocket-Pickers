using System;
using _Scripts.Enemies.ViewTypes;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Sniper
{
    public class SniperSettings : MonoBehaviour, IEnemySettings
    {
         [Header("General Settings")]
        public bool isFacingRight = true;

        public bool IsFacingRight() => isFacingRight;

        [Header("Charge Settings")] 
        public float maxChargeTime = 2.5f;
        public float chargeTime; // Charge time in seconds
        public float reloadTime = 2f;
        public float trackingSpeed = 15f;

        [Header("False Trigger Investigation Settings")]
        public float investigationTime = 3f; // Time in seconds the sniper will spend looking at the false trigger

        #region SinModifiers
        private float _viewModifier = 1.0f; // View width, radius, length, etc modifier for detecting the player
        public event Action<float> OnViewModifierChanged;
        public float ViewModifier
        {
            get => _viewModifier;
            set
            {
                if (!(Math.Abs(_viewModifier - value) > 0)) return;
                _viewModifier = value;
                OnViewModifierChanged?.Invoke(_viewModifier);  // Trigger event when viewModifier is modified
            }
        }
        #endregion

        [Header("Physics Settings")]
        public float maxFallSpeed = 20f;
        public float gravity = 10f;

        [Header("Ground Detection")]
        public float groundCheckDistance = 0.5f;
        public LayerMask groundLayer;
        public Color groundRayColor = Color.red;
        private bool _isGrounded;
        
        public float disabledTimeout = 5f; // Amount of time in seconds the sniper patroller will spend disabled

        public bool IsGrounded() => _isGrounded;

        private Rigidbody2D _rb;
        private IViewType[] _viewTypes;

        private bool isSubscribedToEvents;

        private void OnEnable()
        {
            setListeners();
            chargeTime = maxChargeTime;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _viewTypes = GetComponents<IViewType>();
        }

        private void Update()
        {
            isFacingRight = transform.localScale.x > 0;
            foreach (var view in _viewTypes)
            {
                view.SetView();
            }
        }

        public void setListeners()
        {
            isSubscribedToEvents = true;
            GameManager.Instance.sinChanged += changeFov;
        }

        public void removeListeners()
        {
            if (!isSubscribedToEvents)
            {
                return;
            }
            
            GameManager.Instance.sinChanged -= changeFov;
            isSubscribedToEvents = false;
        }
        private void OnValidate()
        {
            if ((isFacingRight && transform.localScale.x < 0) || (!isFacingRight && transform.localScale.x > 0))
            {
                FlipLocalScale();
            }
        }

        public void HandleGroundDetection()
        {
            var hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
            _isGrounded = hit.collider != null;
        }

        public void HandleGravity()
        {
            if (_isGrounded) return;

            // Apply gravity to make the guard fall
            var newYVelocity = Mathf.MoveTowards(_rb.velocity.y, -maxFallSpeed, gravity * Time.deltaTime);
            _rb.velocity = new Vector2(_rb.velocity.x, newYVelocity);
        }

        public void FlipLocalScale()
        {
            var localScale = transform.localScale.y;
            if (transform.localScale.x > 0f)
            {
                localScale = -localScale;
            }
            else
            {
                localScale = Mathf.Abs(localScale);
            }
            // transform.localScale.x = localScale;
            transform.localScale = new Vector3(localScale, transform.localScale.y, transform.localScale.z);
        }

        
        // Gizmos of the patrol distance and ground detection ray visible in the editor
        private void OnDrawGizmos()
        {
            Gizmos.color = groundRayColor;

            // Starting point of the ray
            var start = transform.position;

            // Ending point of the ray
            var end = start + Vector3.down * groundCheckDistance;

            // Draw the ground detection ray
            Gizmos.DrawLine(start, end);

            // Optionally, draw a sphere at the end point to make it more visible
            Gizmos.DrawSphere(end, 0.1f);
        }

        public void changeFov()
        {
            gameObject.GetComponent<RayView>().ChangeView();
            //todo change charge time to go from between 2.5f - 0.5f
            //difference is 2.0
            //we need to turn the range of 0-135 to 0-1.5

            int sinModifier = PlayerVariables.Instance.sinHeld;
            if (sinModifier >= 135) sinModifier = 135;

            //turns range of 0-135 to be 0-2.0f
            float chargeModifier = (float)sinModifier / 67.5f;
            
            chargeTime = maxChargeTime - chargeModifier;
        }
    }
}
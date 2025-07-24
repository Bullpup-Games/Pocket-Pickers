using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Enemies.Guard;
using _Scripts.Enemies.Guard.State;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.ViewTypes
{
    public class SphereView : MonoBehaviour, IViewType
    {
        [Header("Detection Settings")]
        [Tooltip("If enabled cuts the detection time down to a quarter")]
        [SerializeField] private bool quickDetection;
        public bool QuickDetection() => quickDetection;
        [Tooltip("Upper bound for the modifier given to the detection timer when the player is at the far edge of the view"),
         Range(1.0f, 10.0f)]
        public float maxDistanceModifier = 3.0f;
        [Tooltip("Lower bound for the modifier given to the detection timer when the player is close to the origin of the view"),
         Range(0.01f, 1.0f)]
        public float minDistanceModifier = 0.25f;
        [Header("Sphere View Settings")]
        public float normalViewRadius = 5f;    // Normal detection radius
        public float alertedViewRadius = 10f;  // Detection radius when alerted
        public LayerMask targetLayer;          // Layer of the player
        public LayerMask environmentLayer;     // Layers considered as obstacles
        public LayerMask enemyLayer;
        public Vector2 offset;                 // Offset from the enemy's position

        public Color color = Color.blue;

        private float _viewRadius;
        private IEnemySettings _settings;
        private IEnemyStateManagerBase _stateManager;
        private bool _playerDetectedThisFrame = false;

        private float _baseNormalViewRadius;
        private float _baseAlertedViewRadius;

        public event Action<bool, float> PlayerDetected;
        public event Action NoPlayerDetected;

        private void Awake()
        {
            InitializeSettings();
        }

        private void Update()
        {
            // Update the view radius based on the enemy's current state
            if (_stateManager.IsAlertedState())
            {
                _viewRadius = alertedViewRadius;
            }
            else
            {
                _viewRadius = normalViewRadius;
            }
        }

        private void OnEnable()
        {
            _settings.OnViewModifierChanged += UpdateView;
        }

        private void OnDisable()
        {
            _settings.OnViewModifierChanged -= UpdateView;
        }

        public void SetView()
        {
            if (_stateManager.IsDisabledState() || _stateManager.IsStunnedState()) return;
            
            UpdateHorizontalOffset();
            
            var position = (Vector2)transform.position + offset;

            // Check for player colliders within the view radius
            var targetsInViewRadius = Physics2D.OverlapCircleAll(position, _viewRadius, targetLayer);

            foreach (var target in targetsInViewRadius)
            {
                var targetPos = (Vector2)target.transform.position;
                var directionToTarget = (targetPos - (Vector2)transform.position).normalized;
                var distanceToTarget = Vector2.Distance(position, targetPos);

                // Check for obstacles between the enemy and the target
                var hit = Physics2D.Raycast(position, directionToTarget, distanceToTarget, environmentLayer);

                if (hit.collider is null)
                {
                    // Target is detected
                    var modifier = CalculateModifier(distanceToTarget);
                    OnTargetDetected(modifier);
                    return;
                }
            }

            OnNoTargetDetected();
        }

        private void OnTargetDetected(float modifier)
        {
            _playerDetectedThisFrame = true;
            PlayerDetected?.Invoke(quickDetection, modifier);
        }

        private void OnNoTargetDetected()
        {
            _playerDetectedThisFrame = false;
            NoPlayerDetected?.Invoke();
        }

        public bool IsPlayerDetectedThisFrame() => _playerDetectedThisFrame;
        
        private float CalculateModifier(float distanceToPlayer)
        {
            var minDistance = 0.5f;
            var maxDistance = _viewRadius;
            distanceToPlayer = Mathf.Clamp(distanceToPlayer, minDistance, maxDistance);

            var t = (distanceToPlayer - minDistance) / (maxDistance - minDistance);
            var modifier = Mathf.Lerp(maxDistanceModifier, minDistanceModifier, t);
            return modifier;
        }

        public void UpdateView(float modifier)
        {
            // Adjust the view radii based on the modifier
            normalViewRadius = _baseNormalViewRadius * modifier;
            alertedViewRadius = _baseAlertedViewRadius * modifier;
        }

        private void OnDrawGizmos()
        {
            if (_settings == null) InitializeSettings();

            UpdateHorizontalOffset();

            Gizmos.color = color;
            var position = (Vector2)transform.position + offset;
            float radius = _viewRadius != 0 ? _viewRadius : normalViewRadius;
            Gizmos.DrawWireSphere(position, radius);
        }

        /*
         * Updates the horizontal offset when the enemy switches directions
         * Adjust this method if the offset depends on facing direction
         */
        private void UpdateHorizontalOffset()
        {
            if (_settings.IsFacingRight())
            {
                offset.x = -Mathf.Abs(offset.x);
            }
            else
            {
                offset.x = Mathf.Abs(offset.x);
            }
        }

        // Initialize components and base view radii
        private void InitializeSettings()
        {
            _settings = GetComponent<IEnemySettings>();
            _stateManager = GetComponent<IEnemyStateManagerBase>();

            if (_settings == null)
            {
                Debug.LogError("EnemySettings component not found on " + gameObject.name);
            }

            // Store base radii for modifiers
            _baseNormalViewRadius = normalViewRadius;
            _baseAlertedViewRadius = alertedViewRadius;
        }

        public void ChangeView()
        {
            //TODO impliment this
            Debug.Log("Haven't implimented this yet");
        }
    }
}
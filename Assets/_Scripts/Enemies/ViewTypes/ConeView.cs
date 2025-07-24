using System;
using System.Collections.Generic;
using _Scripts.Enemies.Guard;
using _Scripts.Enemies.Guard.State;
using _Scripts.Player;
using UnityEngine;


namespace _Scripts.Enemies.ViewTypes
{
    public class ConeView : MonoBehaviour, IViewType
    {
        
       /*
       The plan:
       x Seperate the default view range from the actual view range
       x in awake, set the actual view range to the default view range
       x in GameManager, add an event action that will go whenever the total sin changes
       In enemy stats, add a function that listens for the event, and calls this function to change range
       In here, change the actual range to be dependent on the amount of sin the player holds
       */
        
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
        
        [Header("Cone View Settings")] 
        public float defaultViewAngle = 45f;
        public float normalViewAngle;
        public float alertedViewAngle; //should be 60 by default, should be 15 more than default or 4/3 of normal
        
        public float defaultViewDistance = 5f;
        public float normalViewDistance;
        public float alertedViewDistance; //should be 10 by default, either double or add 5 to normal
        public LayerMask targetLayer;
        public LayerMask environmentLayer;
        public Vector2 offset; // (0.4, 1.0) for current enemy model

        private float _viewAngle;
        private float _viewDistance;
        public float GetCurrentViewAngle() => _viewAngle;
        public float GetCurrentViewDistance() => _viewDistance;
        private IEnemySettings _settings;
        private IEnemyStateManagerBase _stateManager;
        private bool _playerDetectedThisFrame = false;

        public event Action<bool, float> PlayerDetected;
        public event Action NoPlayerDetected;
        private void Awake()
        {
            InitializeSettings(); 
        }

        private void Update()
        {
            if (_stateManager.IsAlertedState())
            {
                _viewAngle = alertedViewAngle;
                _viewDistance = alertedViewDistance;
            }
            else
            {
                _viewAngle = normalViewAngle;
                _viewDistance = normalViewDistance;
            }
        }

        private void OnEnable()
        {
            _settings.OnViewModifierChanged += UpdateView;
            normalViewAngle = defaultViewAngle;
            normalViewDistance = defaultViewDistance;
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
            var direction = _settings.IsFacingRight() ? Vector2.right : Vector2.left;

            // check for player collider within the view distance
            var targetsInViewRadius = Physics2D.OverlapCircleAll(position, _viewDistance, targetLayer);

            Vector3 directionToTarget;
            foreach (var target in targetsInViewRadius)
            {
                var targetPos = target.transform.position;
                directionToTarget = (targetPos - transform.position).normalized;
                var distanceToTarget = Vector2.Distance(position, targetPos);

                // Check if the target is within the adjusted view angle
                var angleBetween = Vector2.Angle(direction, directionToTarget);

                if (!(angleBetween < _viewAngle / 2)) continue;
                // TODO: When adding variable detection lengths based on distance get the distance from here
                // Check for obstacles between the enemy and the target
                var obstacleHit = Physics2D.Raycast((Vector2)transform.position, directionToTarget, _viewDistance, environmentLayer);

                if (obstacleHit.collider == null)
                {
                    // Target is detected
                    // Debug.Log("Target in Line of Sight " + target.gameObject);
                    // Debug.Log("cone hit");
                    var modifier = CalculateModifier(distanceToTarget);
                    OnTargetDetected(modifier);
                    return;
                }
            }
            
            // Debug.Log("Direction:" + directionToTarget);
            OnNoTargetDetected();
        }

        public void ChangeView()
        {
            int sinModifier = PlayerVariables.Instance.sinHeld;
            
            
            //change the angle of detection
            //we don't want the angle to be any more than 180 degrees. 180 - 45 (default angle) is 135
            if (sinModifier >= 135)
            {
                sinModifier = 135;
            }
            normalViewAngle = defaultViewAngle + sinModifier;
            alertedViewAngle = (normalViewAngle * 4) / 3;
            
            //we don't want the anle to be any more than 180 degrees
            if (alertedViewAngle > 180)
            {
                alertedViewAngle = 180;
            }
            
            //change the distance of detection
            
            //we want to have the distance be between 5 and 10 for non aggro, and between 10 and 15 for aggro
            //dividing by 27 will transpose our range from being between 0 -> 135, to be between 0 -> 5
            //adding 5 will change it to be between 5 -> 10
            normalViewDistance = (sinModifier / 27) + 5;
            alertedViewDistance = normalViewDistance + 5;
        }
        private void OnTargetDetected(float modifier)
        {
            _playerDetectedThisFrame = true;
            PlayerDetected?.Invoke(quickDetection, modifier);
        }

        private void OnNoTargetDetected()
        {
            // Debug.Log("No target found");
            _playerDetectedThisFrame = false;
            NoPlayerDetected?.Invoke();
        }
        
        public bool IsPlayerDetectedThisFrame() => _playerDetectedThisFrame;
        
        private float CalculateModifier(float distanceToPlayer)
        {
            var minDistance = 0.5f;
            var maxDistance = _viewDistance;
            distanceToPlayer = Mathf.Clamp(distanceToPlayer, minDistance, maxDistance);

            var t = (distanceToPlayer - minDistance) / (maxDistance - minDistance);
            // Inverse Lerp to get modifier between 2 and 0.5
            var modifier = Mathf.Lerp(maxDistanceModifier, minDistanceModifier, t);
            return modifier;
        }
        
        public void UpdateView(float modifier)
        {
             _viewAngle *= modifier;
        }

        private void OnDrawGizmos()
        {
            if (_settings == null) InitializeSettings();
            
            UpdateHorizontalOffset();
            
            Gizmos.color = Color.red;
            
            var position = (Vector2)transform.position + offset;
            var direction = _settings.IsFacingRight() ? Vector2.right : Vector2.left;

            var leftBoundary = Quaternion.Euler(0, 0, _viewAngle / 2) * direction;
            var rightBoundary = Quaternion.Euler(0, 0, -_viewAngle / 2) * direction;

            Gizmos.DrawLine(position, position + (Vector2)(leftBoundary * _viewDistance));
            Gizmos.DrawLine(position, position + (Vector2)(rightBoundary * _viewDistance));
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, _viewDistance);
        }

        /*
         * Updates the horizontal offset of the cone when the guard switches directions
         * If this isn't done the cone will be in an improper offset on one side or the other unless its origin
         * is exactly the same as the enemy's horizontal origin
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
        
        // Wrapping GetComponent calls here so they can be called for the Gizmos in editor mode
        private void InitializeSettings()
        {
            _settings = GetComponent<IEnemySettings>();
            _stateManager = GetComponent<IEnemyStateManagerBase>();
            if (_settings == null)
            {
                Debug.LogError("GuardSettings component not found on " + gameObject.name);
            }
        }
    }
}
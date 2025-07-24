using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Card;
using _Scripts.Enemies.Sniper.State;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies.ViewTypes
{
    [RequireComponent(typeof(SniperStateManager))]
    public class RayView : MonoBehaviour, IViewType
    {
        [Tooltip(
            "Upper bound for the modifier given to the detection timer when the player is at the far edge of the view")]
        public float maxSweepModifier = 3f;

        [Tooltip(
            "Lower bound for the modifier given to the detection timer when the player is close to the origin of the view")]
        public float minSweepModifier = 1f;

        [Header("Ray View Settings")]
        public float sweepAngle = 90f; // Total sweep angle
        public float defaultSweepSpeed = 10f;// Degrees per second
        public float sweepSpeed;//with modifiers
        public float maxRayDistance = 100f; // We want the ray to basically be infinite but to define the line renderer & gizmos a max is needed, just keep it set high
        public Vector2 offset; // Ray origin offset
        public LayerMask playerLayer;
        public LayerMask enemyLayer;
        public LayerMask environmentLayer;

        private IEnemySettings _settings;
        private SniperStateManager _stateManager;
        private bool _playerDetectedThisFrame = false;
        private bool _playerDetectedLastFrame = false;
        private List<Collider2D> _enemiesDetected;
        
        private float _currentAngle;
        private Vector2 _lastKnownPlayerPosition;
        private Transform _playerTransform;
        public LineRenderer lineRenderer;
        
        /*
         * State booleans, default behavior is to sweep vertically.
         * When a player is spotted the ray will follow the player's position
         * If the player manages to get away and hide the shot will be fired at their last known position.
         * When reloading or disabled the ray will not be shown and have no behavior.
         */
        private bool _isSweeping = true;
        private bool _isTrackingPlayer = false;
        private bool _isTrackingLastKnownPosition = false;
        private bool _isDisabled = false;
        private bool _lookingAtFalseTrigger = false;

        public bool ignoreSweepAngle; // Used to disable checks for the sweep angle, allowing the sniper to lock on where ever 

        public event Action<bool, float> PlayerDetected;
        public event Action NoPlayerDetected;
        
        // Ray view is set up specifically for the sniper, which doesn't have a detection mode.
        // This could potentially be used for a quick charge instead, but would likely feel very unfair.
        public bool QuickDetection() => false;

        private void Awake()
        {
            InitializeSettings();
            // Start at the lower bound of the sweep
            _currentAngle = -sweepAngle / 2;

            _enemiesDetected = new List<Collider2D>();
        }

        private void Update()
        {
            if (_stateManager.IsDisabledState() || _stateManager.IsReloadingState())
            {
                EnterDisabledState();
                return;
            }

            if (_stateManager.alertedFromAggroSkreecher)
            {
                TrackPlayer();
                return;
            }

            if (_stateManager.alertedFromInvestigatingSkreecher)
            {
                _stateManager.investigatingFalseTrigger = true;
                _stateManager.TransitionToState(_stateManager.ChargingState);
                _stateManager.alertedFromInvestigatingSkreecher = false;
            }

            if (ignoreSweepAngle)
            {
                if ((PlayerVariables.Instance.transform.position.x > transform.position.x && !_stateManager.Settings.isFacingRight) ||
                    (PlayerVariables.Instance.transform.position.x < transform.position.x && _stateManager.Settings.isFacingRight))
                    _stateManager.Settings.FlipLocalScale();
                TrackPlayer();
                CastRay();
                return;
            }
            
            if (_stateManager.investigatingFalseTrigger)
            {
                LookAtFalseTrigger();
                CastRay();

                // If the player crosses the ray while the sniper is investigating it will switch to tracking the player
                // if (_playerDetectedThisFrame)
                //     _stateManager.TransitionToState(_stateManager.ChargingState);
                // else
                //     return;
                return;
            }
            
            // The ray is disabled but the enemy is back in a normal state (patrolling, charging, etc)
            if (_isDisabled)
                ExitDisabledState();

            UpdateStateBooleans();

            if (_isSweeping)
                Sweep();
            else if (_isTrackingPlayer)
                TrackPlayer();
            else if (_isTrackingLastKnownPosition)
                TrackLastKnownPosition();


            CastRay();

            _playerDetectedLastFrame = _playerDetectedThisFrame;
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
            /* Intentionally empty */
        }

        #region State Management

        private void UpdateStateBooleans()
        {
            if (_stateManager.IsDisabledState() || _stateManager.IsReloadingState())
            {
                EnterDisabledState();
            }
            else
            {
                // Player was detected this frame, track them
                if (_playerDetectedThisFrame)
                {
                    _isSweeping = false;
                    _isTrackingPlayer = true;
                    _isTrackingLastKnownPosition = false;
                }
                // Player was detected last frame but not this one, track the last known position
                else if (_playerDetectedLastFrame && !_playerDetectedThisFrame)
                {
                    _lastKnownPlayerPosition = _playerTransform.position;
                    // Track the last known position until the shot is fired
                    if (_stateManager.IsChargingState())
                    {
                        _isSweeping = false;
                        _isTrackingPlayer = false;
                        _isTrackingLastKnownPosition = true;
                    }
                    // Shot fired, prepare sweeping state
                    else
                    {
                        _isSweeping = true;
                        _isTrackingPlayer = false;
                        _isTrackingLastKnownPosition = false;
                    }
                }
                // Exit tracking last known position when not in Charging State
                else if (_isTrackingLastKnownPosition && !_stateManager.IsChargingState())
                {
                    _isSweeping = true;
                    _isTrackingLastKnownPosition = false;
                }
            }
        }

        private void EnterDisabledState()
        {
            if (!_isDisabled)
            {
                _isDisabled = true;
                lineRenderer.enabled = false;
                _isSweeping = false;
                _isTrackingPlayer = false;
                _isTrackingLastKnownPosition = false;
            }
        }

        private void ExitDisabledState()
        {
            _isDisabled = false;
            lineRenderer.enabled = true;
            _isSweeping = true;
        }

        #endregion

        #region Behavior Methods

        private void Sweep()
        {
            // Update the angle over time to sweep back and forth when a player is not in sight
            _currentAngle += sweepSpeed * Time.deltaTime;

            if (_currentAngle > sweepAngle / 2)
            {
                _currentAngle = sweepAngle / 2;
                sweepSpeed = -Mathf.Abs(sweepSpeed);
            }
            else if (_currentAngle < -sweepAngle / 2)
            {
                _currentAngle = -sweepAngle / 2;
                sweepSpeed = Mathf.Abs(sweepSpeed);
            }
        }

        private void TrackPlayer()
        {
            if (_stateManager.alertedFromAggroSkreecher)
            {
                _playerTransform = PlayerVariables.Instance.transform;
                _stateManager.alertedFromAggroSkreecher = false;
            }
            
            if (_playerTransform is null)
            {
                // Lost player reference, switch states will handle the transition
                return;
            }

            var position = (Vector2)transform.position;
            var directionToPlayer = ((Vector2)_playerTransform.position - position).normalized;
            var angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            var facingDirectionAngle = _settings.IsFacingRight() ? 0f : 180f;
            var targetAngleDifference = Mathf.DeltaAngle(facingDirectionAngle, angleToPlayer);
            
            // Player is outside of the sweep angle, resume sweeping
            if (Mathf.Abs(targetAngleDifference) > sweepAngle / 2 && !ignoreSweepAngle)
            {
                _isSweeping = true;
                _isTrackingPlayer = false;
                _playerTransform = null;
                return;
            }

            // Smooth Lerp when tracking the player instead of snap-locking
            _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngleDifference, _stateManager.Settings.trackingSpeed * Time.deltaTime);
        }

        private void TrackLastKnownPosition()
        {
            var position = (Vector2)transform.position;
            var directionToLastKnown = (_lastKnownPlayerPosition - position).normalized;
            var angleToLastKnown = Mathf.Atan2(directionToLastKnown.y, directionToLastKnown.x) * Mathf.Rad2Deg;

            var facingDirectionAngle = _settings.IsFacingRight() ? 0f : 180f;
            var angleDifference = Mathf.DeltaAngle(facingDirectionAngle, angleToLastKnown);

            // Last known position is outside of the sweep angle
            // Keep the angle at the boundary
            if (Mathf.Abs(angleDifference) > sweepAngle / 2)
                _currentAngle = Mathf.Sign(angleDifference) * (sweepAngle / 2);
            else
                _currentAngle = angleDifference;
        }

        private void LookAtFalseTrigger()
        {
            var position = (Vector2)transform.position;
            var falseTriggerPosition = CardManager.Instance.lastFalseTriggerPosition;
            var directionToFalseTrigger = (falseTriggerPosition - position).normalized;
            var angleToFalseTrigger = Mathf.Atan2(directionToFalseTrigger.y, directionToFalseTrigger.x) * Mathf.Rad2Deg;

            var facingDirectionAngle = _settings.IsFacingRight() ? 0 : 180f;
            var angleDifference = Mathf.DeltaAngle(facingDirectionAngle, angleToFalseTrigger);
            
            // Check for out of bounds intentionally left out, false trigger will be out of the sight bounds a lot, but
            // it should still be able to be used as a distraction.

            _currentAngle = angleDifference;
        }

        #endregion

        private void CastRay()
        {
            if (_isDisabled)
                return;

            UpdateHorizontalOffset();
            var position = (Vector2)transform.position + offset;

            // Calculate the direction of the ray based on the current angle
            var facingDirection = _settings.IsFacingRight() ? 0f : 180f;
            var angle = facingDirection + _currentAngle;
            var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            
            // Clear enemies detected list from last frame,
            // Enemies detected keeps track of which enemies are currently in the sniper's LOS
            _enemiesDetected.Clear();

            // Cast the current ray position looking for player, enemies & environment hits
            var hits = Physics2D.RaycastAll(position, direction, maxRayDistance, playerLayer | enemyLayer | environmentLayer);

            var playerDetected = false;
            var hitPoint = position + direction * maxRayDistance;

            foreach (var hit in hits)
            {
                var hitLayer = hit.collider.gameObject.layer;

                // Hit the environment, stop processing further hits
                if (((1 << hitLayer) & environmentLayer) != 0)
                {
                    hitPoint = hit.point;
                    break;
                }
                
                // Hit the player, update flags and keep processing further hits? TODO: Decide if stopping the ray here is better or not
                if (((1 << hitLayer) & playerLayer) != 0)
                {
                    var distanceToPlayer = Vector2.Distance(position, hit.point);
                    var modifier = CalculateModifier(distanceToPlayer);
                    OnTargetDetected(modifier, hit.collider.transform);
                    playerDetected = true;
                    hitPoint = hit.point;
                }
                // Hit an enemy, add to list and keep processing further hits
                else if (((1 << hitLayer) & enemyLayer) != 0)
                {
                    // Don't want the sniper to shoot himself in the face...
                    if (hit.collider.gameObject != gameObject)
                        _enemiesDetected.Add(hit.collider);
                }
            }

            if (!playerDetected)
                OnNoTargetDetected();

            // Update the line renderer to the furthest hit point
            lineRenderer.SetPosition(0, position);
            lineRenderer.SetPosition(1, hitPoint);

            if (hits.Length != 0) return;
            
            // If there were no hits at all update line renderer to 'max' distance
            lineRenderer.SetPosition(0, position);
            lineRenderer.SetPosition(1, position + direction * maxRayDistance);
            OnNoTargetDetected();
        }

        private void OnTargetDetected(float modifier, Transform playerTransform)
        {
            _playerDetectedThisFrame = true;
            PlayerDetected?.Invoke(false, modifier);

            _playerTransform = playerTransform;
            _lastKnownPlayerPosition = playerTransform.position;
        }

        private void OnNoTargetDetected()
        {
            _playerDetectedThisFrame = false;
            NoPlayerDetected?.Invoke();
        }

        public List<Collider2D> EnemiesDetected() => _enemiesDetected;

        public bool IsPlayerDetectedThisFrame() => _playerDetectedThisFrame;

        
        private float CalculateModifier(float distanceToPlayer)
        {
            /*
             * TODO: Figure out if this should be affected by distance to the player or left alone.
             * Other views have detection speed modifiers based on distance so it would make sense to use here as well, but
             * it would be for the charge time instead of the detection speed, which could easily get out of hand and be hard to balance.
             */ 
            return 1f;
        }
        
        public void UpdateView(float modifier)
        {
            // sweepSpeed *= modifier;
        }

        private void OnDrawGizmos()
        {
            if (_settings == null) InitializeSettings();

            UpdateHorizontalOffset();
            var position = (Vector2)transform.position + offset;

            // Draw the sweep arc
            Gizmos.color = Color.yellow;
            var facingDirection = _settings.IsFacingRight() ? 0f : 180f;
            var leftBoundary = new Vector2(Mathf.Cos((facingDirection - sweepAngle / 2) * Mathf.Deg2Rad),
                Mathf.Sin((facingDirection - sweepAngle / 2) * Mathf.Deg2Rad));
            var rightBoundary = new Vector2(Mathf.Cos((facingDirection + sweepAngle / 2) * Mathf.Deg2Rad),
                Mathf.Sin((facingDirection + sweepAngle / 2) * Mathf.Deg2Rad));

            Gizmos.DrawLine(position, position + leftBoundary * maxRayDistance);
            Gizmos.DrawLine(position, position + rightBoundary * maxRayDistance);

            // Draw the current ray
            Gizmos.color = Color.blue;
            var direction = new Vector2(Mathf.Cos((facingDirection + _currentAngle) * Mathf.Deg2Rad),
                Mathf.Sin((facingDirection + _currentAngle) * Mathf.Deg2Rad));
            Gizmos.DrawLine(position, position + direction * maxRayDistance);
        }

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

        private void InitializeSettings()
        {
            _settings = GetComponent<IEnemySettings>();
            _stateManager = GetComponent<SniperStateManager>();
            lineRenderer = GetComponent<LineRenderer>();
            if (_settings == null)
            {
                Debug.LogError("EnemySettings component not found on " + gameObject.name);
            }

            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // TODO: Maybe change to a custom material?
            ResetLineRendererColor();
            sweepSpeed = defaultSweepSpeed;
        }

        public void ResetLineRendererColor()
        {
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
        }

       public void ChangeToColor(Color targetColor, float duration)
       {
           StartCoroutine(ChangeColorCoroutine(targetColor, duration));
       }

       private IEnumerator ChangeColorCoroutine(Color newColor, float duration)
       {
           var initialStartColor = lineRenderer.startColor;
           var initialEndColor = lineRenderer.endColor;
           var time = 0f;

           // Lerp between the old and new color
           while (time < duration)
           {
               time += Time.deltaTime;
               var t = time / duration;
               lineRenderer.startColor = Color.Lerp(initialStartColor, newColor, t);
               lineRenderer.endColor = Color.Lerp(initialEndColor, newColor, t);
               yield return null;
           }

           // Make sure the final color is set to the exact given value
           lineRenderer.startColor = newColor;
           lineRenderer.endColor = newColor;
       }

        public void ChangeView()
        {
            //the default sweep speed is 10. We want its maximum to be 30. We need the modifier to go between 0 and 20
            int sinModifier = PlayerVariables.Instance.sinHeld;

            if (sinModifier >= 135)
            {
                sinModifier = 135;
            }
            //transposes the range of 0-135 to the range of 0-20
            float speedModifier = (float)sinModifier / 6.75f;

            sweepSpeed = defaultSweepSpeed + speedModifier;
        }
    }
}
using System;
using _Scripts.Enemies;
using _Scripts.Enemies.Guard.State;
using _Scripts.Enemies.Skreecher.State;
using _Scripts.Enemies.Sniper;
using _Scripts.Enemies.Sniper.State;
using _Scripts.Player;
using _Scripts.Sound;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Card
{
    public class Card : MonoBehaviour
    {
        [Header("False Trigger Settings")]
        [SerializeField] private float falseTriggerRadius = 4f;
        [SerializeField] private Color gizmoColor = Color.cyan;

        public float speed = 20f; // Speed of the card
        public int totalBounces;  // Total allowed bounces
        public int bounces;       // Current number of bounces
        
       public CardEffectHandler effectHandler;

        private Vector2 _direction;   // Current movement direction
        private Vector2 _velocity;    // Current velocity
        private float _startTime;    // Time when the card was instantiated

        private Rigidbody2D _rb;
        private Vector2 _previousPosition; // Position in the previous frame
        public Vector2 lastSafePosition; // Keep track of safe positions to teleport as the card travels

        private const float MinMoveDistance = 0.01f; // Minimum movement distance to prevent sticking

        #region Singleton

        public static Card Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(Card)) as Card;

                return _instance;
            }
            set => _instance = value;
        }

        private static Card _instance;

        #endregion

        private void OnEnable()
        {
            SetListeners();
            effectHandler = CardEffectHandler.Instance;
        }

        private void OnDestroy()
        {
            DeleteListeners();
        }

        private void SetListeners()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnFalseTrigger += ActivateFalseTrigger;
                InputHandler.Instance.OnCancelActiveCard += CancelCardThrow;
            }

            if (CardManager.Instance != null)
            {
                CardManager.Instance.Teleport += CatchTeleport;
            }
        }

        private void DeleteListeners()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnFalseTrigger -= ActivateFalseTrigger;
                InputHandler.Instance.OnCancelActiveCard -= CancelCardThrow;
            }

            if (CardManager.Instance != null)
            {
                CardManager.Instance.Teleport -= CatchTeleport;
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            bounces = 0;

            // Ignore collisions with the player to avoid pushing on Awake frame
            var playerCollider = PlayerVariables.Instance.gameObject.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>());

            lastSafePosition = PlayerVariables.Instance.transform.position;
            _startTime = Time.time;
            
            // Calculate initial velocity
            _direction = HandleCardStanceArrow.Instance.currentDirection.normalized;
            CalculateVelocity(_direction);
        }

        public void Launch(Vector2 direction)
        {
            _direction = direction.normalized;
            CalculateVelocity(_direction);
        }

        private void CalculateVelocity(Vector2 direction)
        {
            _velocity = direction * speed;
        }

        private void Update()
        {
            // Destroy the card after its lifetime expires
            if (Time.time - _startTime >= CardManager.Instance.cardLifeTime)
            {
                DestroyCard();
            }
        }

        private void FixedUpdate()
        {
            MoveCard();
            UpdateSafePosition();
        }

        private void MoveCard()
        {
            // Store the previous position
            _previousPosition = transform.position;

            // Calculate movement
            var movement = _velocity * Time.fixedDeltaTime;
            var newPosition = _previousPosition + movement;

            CheckForHit(movement, ref newPosition);
            // If movement amount is too small skip to prevent sticking
            if (movement.magnitude < MinMoveDistance)
            {
                movement = _direction * MinMoveDistance;
                newPosition = _previousPosition + movement;
            }

            // Move the card to the new position
            transform.position = newPosition;
        }

        private void CheckForHit(Vector2 movement, ref Vector2 newPosition)
        {
            // Perform a raycast from previousPosition to newPosition to check for collisions in between frames
            var hit = Physics2D.Raycast(
                _previousPosition,
                movement.normalized,
                movement.magnitude,
                LayerMask.GetMask("Environment","Enemy")
            );

            

            if (!hit.collider.IsUnityNull())
            {
                
                /*
                 * This is here so that eventually, I can move all layer interactions into here.
                 * We want to check to make sure that we can get the layer from a hit, and interact with
                 * it by name
                 * LayerMask.LayerToName(5)
                 * takes a layer id number and returns its name
                 * LayerMask.NameToLayer("Layer Name")
                 * takes a layer name and returns its id number
                 */
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
                {
                    CollideWithWall(hit, ref newPosition);
                    return;
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    CollideWithEnemy(hit);
                    return;
                }
                
                
            }
        }
        /*
         * Keeps track of the last known safe position to teleport to each frame the card is alive
         * to prevent teleportation into walls or objects
         */
        private void UpdateSafePosition()
        {
            //TODO make it so if its not immediately a safe position, it will test the offsets
            /*
             * Test if the current position is immediately viable. If it is, set it as the safe position
             * If it is not, have a horizontal and a vertical offset based on the player hitbox
             * Test if any of the offsets would be a valid position
             * If none of them are valid, don't update the safe position
             * If there is a valid teleport, figure out the smallest offset that will let you fit
             */
            
            // var playerCollider = PlayerVariables.Instance.Collider2D;
            // var playerBoundsSize = playerCollider.bounds.size;
            // var percent = 0.85f; // Only use 85% of the player's collider size, feels better getting into tight spots and doesn't clip
            // var colliderSize = new Vector2(playerBoundsSize.x * percent, playerBoundsSize.y * percent);
            // var colliderOffset = playerCollider.offset;
            //
            // // get the current collider position with the player offset to cast from when checking for hits
            // var colliderPos = (Vector2)transform.position + colliderOffset;
            //
            // // Check for obstructions that would prevent a teleport in this location
            // var hits = Physics2D.OverlapBoxAll(
            //     colliderPos,
            //     colliderSize,
            //     0f,
            //     LayerMask.GetMask("Environment", "Enemy")
            // );

            Vector2 leftXOffset = new Vector2(-0.265f,0);
            Vector2 rightXOffset = new Vector2(0.265f,0);
            Vector2 yOffset = new Vector2(0,-1.2f);
            
            //run the bounds check with standard bounds
            var hits = boundsCheck(Vector2.zero);
            if (hits.Length == 0)
            {
                 lastSafePosition = transform.position;
                 return;//we will not run anything else
            } 
            
            hits = boundsCheck(rightXOffset);
            if (hits.Length == 0)
            {
                lastSafePosition = transform.position + (Vector3)rightXOffset;
                return;//we will not run anything else
            } 
            
            hits = boundsCheck(leftXOffset);
            if (hits.Length == 0)
            {
                lastSafePosition = transform.position + (Vector3)leftXOffset;
                return;//we will not run anything else
            } 
            
            hits = boundsCheck(yOffset);
            if (hits.Length == 0)
            {
                lastSafePosition = transform.position + (Vector3)yOffset;
                return;//we will not run anything else
            }
            
            hits = boundsCheck((leftXOffset + yOffset));
            if (hits.Length == 0)
            {
                lastSafePosition = transform.position + (Vector3)(leftXOffset + yOffset);
                return;//we will not run anything else
            }
            
            hits = boundsCheck((rightXOffset + yOffset));
            if (hits.Length == 0)
            {
                lastSafePosition = transform.position + (Vector3)(rightXOffset + yOffset);
                return;//we will not run anything else
            }

            //lastSafePosition = Vector2.zero;

            //if it is not immediately safe, test if there is a different safe position
            //x offset is .275
            //y offset starts at the feet, so we don't need to check down
            //y offset is 1.14
        }

        private Collider2D[] boundsCheck(Vector2 offset)
        {
            var playerCollider = PlayerVariables.Instance.Collider2D;
            var playerBoundsSize = playerCollider.bounds.size;
            var percent = 0.85f; // Only use 85% of the player's collider size, feels better getting into tight spots and doesn't clip
            var colliderSize = new Vector2(playerBoundsSize.x * percent, playerBoundsSize.y * percent);
            var colliderOffset = playerCollider.offset;
            
            // get the current collider position with the player offset to cast from when checking for hits
            var colliderPos = (Vector2)transform.position + colliderOffset + offset;
            
            // Check for obstructions that would prevent a teleport in this location
            var hits = Physics2D.OverlapBoxAll(
                colliderPos,
                colliderSize,
                0f,
                LayerMask.GetMask("Environment", "Enemy")
            );
            return hits;
        }

        private void ActivateFalseTrigger()
        {
            if (CardManager.Instance.falseTriggerOnCooldown)
            {
                Debug.Log("False Trigger on cooldown.");
                return;
            }

            // Update the last false trigger position
            CardManager.Instance.lastFalseTriggerPosition = transform.position;
            Debug.Log("False Trigger Activated");
            
            //activate the animation
            CardEffectHandler.Instance.FalseTriggerEffect(gameObject.transform.position);
            // Play sound effect
            CardSoundEffectManager.Instance.PlayFalseTriggerClip();
            
            
            // Switch states of all enemies within the false trigger radius
            var colliders = Physics2D.OverlapCircleAll(transform.position, falseTriggerRadius, LayerMask.GetMask("Enemy"));
            foreach (var col in colliders)
            {
                // Attempt to cast to GuardStateManager type
                var guardStateManager = col.GetComponent<IEnemyStateManager<GuardStateManager>>();
                if (guardStateManager != null)
                {
                    guardStateManager.TransitionToState(col.GetComponent<GuardStateManager>().InvestigatingState);
                    CardManager.Instance.ActivateFalseTriggerCooldown();
                }

                // Attempt to cast to SniperStateManager type
                var sniperStateManager = col.GetComponent<IEnemyStateManager<SniperStateManager>>();
                if (sniperStateManager != null)
                {
                    col.GetComponent<SniperStateManager>().investigatingFalseTrigger = true;
                    sniperStateManager.TransitionToState(col.GetComponent<SniperStateManager>().ChargingState);
                    CardManager.Instance.ActivateFalseTriggerCooldown();
                }
                
                // Attempt to cast to SkreecherStateManager type
                var skreecherStateManager = col.GetComponent<IEnemyStateManager<SkreecherStateManager>>();
                if (skreecherStateManager != null)
                {
                    skreecherStateManager.TransitionToState(col.GetComponent<SkreecherStateManager>().InvestigatingState);
                    CardManager.Instance.ActivateFalseTriggerCooldown();
                }
                
            }
            DestroyCard();
            return;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            // Ignore collisions with the player
            if (col.gameObject.CompareTag("Player"))
            {
                CollideWithPlayer(col);
                return;
            }
            
            if (col.gameObject.CompareTag("permeable"))
            {
                //this tag exists because we want the player and enemies to be stopped
                //by permeable objects, but not the card.
                CollideWithPermeable(col);
                return;
            }
            
            if (col.gameObject.CompareTag("EscapeRout"))
            {
                DestroyCard();
            }
            //if it collides with something else, we don't want it to do anything
            return;
        }

        #region CollisionManagement

        private void CollideWithWall(RaycastHit2D hit, ref Vector2  newPosition)
        {
                
                    
            // Adjust position to point of collision
            newPosition = hit.point;

            // Reflect the direction off the hit normal
            var newDirection = Vector2.Reflect(_direction, hit.normal).normalized;

            _direction = newDirection;
            CalculateVelocity(_direction);

            /*
                 When you hit a corner, it will count as 2 bounces because it bounces off of both corners
                 at the same time
            */
            bounces++;

            // Safety check if we entered with max bounces
            if (bounces >= totalBounces)
            {
                effectHandler.DestroyEffect(gameObject.transform.position);
                CardSoundEffectManager.Instance.PlayCardDestroyClip();

                DestroyCard();
                return;
            }
            
            CardSoundEffectManager.Instance.PlayCardHitClip();
            effectHandler.bounceEffect(gameObject.transform.position);
            // Adjust position slightly along the new direction to prevent immediate re-collision
            newPosition += _direction * MinMoveDistance;

            Debug.Log("Reflected off Environment. New direction: " + _direction);
        }

        private void CollideWithPlayer(Collision2D col)
        {
            Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
        }

        private void CollideWithEnemy(RaycastHit2D hit)
        {
            Physics2D.IgnoreCollision(hit.collider, GetComponent<Collider2D>());

            // Attempt to cast to GuardStateManager type
            var guardStateManager = hit.collider.GetComponent<IEnemyStateManager<GuardStateManager>>();
            if (guardStateManager != null)
            {
                Debug.Log("Guard State Manager Found");
                guardStateManager.KillEnemy();
                Debug.Log("Guard Killed");
                CardSoundEffectManager.Instance.PlayEnemyHitClip();
                DestroyCard();
                return;
            }

            // Attempt to cast to SniperStateManager type
            var sniperStateManager = hit.collider.GetComponent<IEnemyStateManager<SniperStateManager>>();
            if (sniperStateManager != null)
            {
                Debug.Log("Sniper State Manager Found");
                sniperStateManager.KillEnemy();
                Debug.Log("Sniper Killed");
                CardSoundEffectManager.Instance.PlayEnemyHitClip();
                DestroyCard();
                return;
            }

            // Attempt to cast to SkreecherStateManager type
            var skreecherStateManager = hit.collider.GetComponent<IEnemyStateManager<SkreecherStateManager>>();
            if (skreecherStateManager != null)
            {
                Debug.Log("Skreecher State Manager Found");
                skreecherStateManager.KillEnemy();
                Debug.Log("Skreecher Killed");
                CardSoundEffectManager.Instance.PlayEnemyHitClip();
                DestroyCard();
                return;
            }
        }

        private void CollideWithPermeable(Collision2D col)
        {
            Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
        }

        #endregion
       
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("EscapeRout"))
            {
                CardEffectHandler.Instance.DestroyEffect(gameObject.transform.position);
                DestroyCard();
            }
        }

        private void CatchTeleport(Vector2 noop)
        {
            DestroyCard();
        }

        // CALL THIS FUNCTION TO DESTROY CARDS, DON'T START FROM THE CARD MANAGER!
        public void DestroyCard()
        {
            // Notify the CardManager that the card has been destroyed
            if (CardManager.Instance != null)
            {
                CardManager.Instance.OnCardDestroyed();
            }
            Destroy(gameObject);
        }

        public void CancelCardThrow()
        {
            CardEffectHandler.Instance.DestroyEffect(gameObject.transform.position);
            CardSoundEffectManager.Instance.PlayCardDestroyClip();

            DestroyCard();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, falseTriggerRadius);
        }
    }
}
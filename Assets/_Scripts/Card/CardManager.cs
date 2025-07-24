using System;
using System.Collections;
using _Scripts.Player;
using _Scripts.Player.State;
using _Scripts.Sound;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Card
{

    /**
    * Card Manager is responsible for instantiating, destroying, and keeping track on the current active card
     * It also tracks subscribes to the EventHandler for information about about card related inputs
     * and sends them out to the necessary scripts.
    */
    
 
    public class CardManager : MonoBehaviour
    {
        public CardEffectHandler effectHandler;
        public InputHandler inputHandler;
        // public PlayerMovementController playerMovementController;
        public HandleCardStanceArrow cardStanceArrow;
        private PlayerStateManager _playerStateManager;

        public Transform player;
        public GameObject cardPrefab;
        private GameObject _instantiatedCard;
        private int _airTimeCounter;

        public bool IsCardInScene() => _instantiatedCard != null;
        
        [Header("Card and Direction Arrow Offsets")]
        public float horizontalOffset = 2.0f;
        public float verticalOffset = 0.3f;

        private float _lastCardThrowTime;  // Track the last time a card was thrown
        
        [Header("Card Behavior Variables")] 
        public float cooldown = 0.5f;
        public float cardSpeed = 20.0f;
        public float cardLifeTime = 10.0f;
        
        public float falseTriggerCooldown = 10f;
        public bool falseTriggerOnCooldown;
        public Vector2 lastFalseTriggerPosition; // Keeps track of the position of the card when false trigger is activated for enemies to investigate

        public event Action <Vector2> Teleport ;
        public event Action cardCreated;
        
        #region Singleton

        public static CardManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(CardManager)) as CardManager;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static CardManager _instance;
        #endregion

        private void OnEnable()
        {
            
            
            // Subscribe to the card throw event
            inputHandler.OnCardThrow += HandleCardAction;
        }

        private void OnDisable()
        {
           
            
            // Unsubscribe from the card throw event
            inputHandler.OnCardThrow -= HandleCardAction;
        }

        private void Update()
        {
            if (PlayerMovement.Instance.IsGrounded() || PlayerMovement.Instance.IsWalled())
            {
                _airTimeCounter = 0; 
            }
        }

        //decide if you should throw card or teleport
        //if you are in the card stance you should throw a card.
        //if not, you should test for teleportation
        private void HandleCardAction()
        {
            if (!PlayerStateManager.Instance.IsStunnedState() &&  _instantiatedCard == null)
            {
                HandleCardThrow();
            }
            else
            {
                HandleTeleportation();
            }
        }

        private bool _cardThrowLimitHit;
        /**
        * Called when the player triggers a card throw, responsible for instantiating a new card
        */
        private void HandleCardThrow()
        {
            
            // Check if the cooldown has passed
            if (Time.time - _lastCardThrowTime <= cooldown)
            {
                // Debug.Log("Card throw is on cooldown.");
                return;
            }

            if (_airTimeCounter >= PlayerVariables.Instance.Stats.AirTimeCardThrowLimit)
            {
                Debug.Log("Airtime limit hit" + _airTimeCounter);
                return;
            }
            _airTimeCounter++;  
            
            // If a card is active, destroy the existing card (this shouldn't occur but is here for safety)
            Card cardScript;
            if (_instantiatedCard != null)
            {
                cardScript = _instantiatedCard.GetComponent<Card>();
                if (cardScript != null)
                {
                    cardScript.DestroyCard();
                }
                else
                {
                    Debug.LogError("Card script not found on active card.");
                    Destroy(_instantiatedCard);  // Fallback: destroy the card if no script is found
                }
                _instantiatedCard = null;  // Reset the active card reference
            }

            // Get the launch direction from the HandleCardStanceArrow
            var currentDirection = HandleCardStanceArrow.Instance.currentDirection;
            
            // If the input direction is zero (a quick throw) set the direction to whichever way the player is facing
            if (currentDirection == Vector2.zero)
                currentDirection = PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;
            
            var angleRad = Mathf.Atan2(currentDirection.y, currentDirection.x);
            
            // Calculate the card's starting position relative to the player
            var offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad) + verticalOffset, 0) * horizontalOffset;
            
            var playerCollider = PlayerVariables.Instance.Collider2D;
            var colliderOffset = playerCollider.offset;
            
            var colliderPos = (Vector2)transform.position + colliderOffset;
            
            var cardSpawnLocation = player.position + (Vector3)colliderPos;

            _instantiatedCard = Instantiate(cardPrefab, cardSpawnLocation, Quaternion.Euler(currentDirection.x, currentDirection.y, 0));
            cardCreated?.Invoke();
            
            
            // Debug.Log("Card Thrown");
            
            // Get the Card script component from the instantiated card to call its Launch function
            cardScript = _instantiatedCard.GetComponent<Card>();

            // Pass the launch direction to the card
            if (cardScript != null)
            {
                cardScript.Launch(currentDirection);
                CardSoundEffectManager.Instance.PlayCardThrowClip();
            }
            else
            {
                Debug.LogError("Card script not found on Card.");
            }

            _lastCardThrowTime = Time.time;
        }

        //if the instance of card is not null, trigger an event that holds the card's
        //transform location. If it is null, do nothing.
        private void HandleTeleportation()
        {
            if (Card.Instance != null)
            {
                if (Card.Instance.lastSafePosition == Vector2.zero)
                {
                    effectHandler.DestroyEffect(Card.Instance.gameObject.transform.position);
                    Card.Instance.DestroyCard();
                    return;
                }
                effectHandler.TeleportEffect(Card.Instance.lastSafePosition);
                Teleport?.Invoke(Card.Instance.lastSafePosition);
                CardSoundEffectManager.Instance.PlayTeleportClip();
            }
        }

        /*
         * Coroutines are tied to the MonoBehavior they are called from
         * so this function is needed to call the cooldown coroutine from the card
         * that gets destroyed immediately after
         */
        
        public void ActivateFalseTriggerCooldown()
        {
            if (falseTriggerOnCooldown) return;
            StartCoroutine(FalseTriggerCooldown());
        }
        private IEnumerator FalseTriggerCooldown()
        {
            falseTriggerOnCooldown = true;
            yield return new WaitForSeconds(falseTriggerCooldown);
            falseTriggerOnCooldown = false;
            yield return null;
        }

        /**
     * Called by the Card script when the card is destroyed or its lifecycle ends
     */
        public void OnCardDestroyed()
        {
           
            _instantiatedCard = null;
            Destroy(_instantiatedCard);
        }
        
        // private void HandleEnterCardStance()
        // {
        //     playerMovementController.EnterCardStance();
        // }
        //
        // private void HandleExitCardStance()
        // {
        //     playerMovementController.ExitCardStance();
        // }

        // private void HandleShowDirectionalArrow()
        // {
        //     cardStanceArrow.InstantiateDirectionalArrow(); 
        // }
        //
        // private void HandleHideDirectionalArrow()
        // {
        //     cardStanceArrow.DestroyDirectionalArrow();
        // }
    }
}
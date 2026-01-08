using UnityEngine;

namespace _Scripts.Player
{
    /// <summary>
    /// Player controller implementing ICardOwner for card throwing and teleportation.
    /// Handles player-specific card interactions and routes input to card system.
    /// </summary>
    public class PlayerController : MonoBehaviour, ICardOwner
    {
        // === DEPENDENCIES ===
        private CardManager _cardManager;

        // === CARD STATE ===
        private float _lastCardThrowTime = -999f; // Initialize to allow immediate first throw
        private const float CardThrowCooldown = 0.5f;

        // === ICARD OWNER IMPLEMENTATION ===

        /// <summary>
        /// Transform property required by ICardOwner interface
        /// </summary>
        Transform ICardOwner.transform
        {
            get => transform;
            set { } // MonoBehaviour transform is read-only, setter is no-op for interface compliance
        }

        /// <summary>
        /// Indicates whether the player can currently throw a card.
        /// Checks both cooldown and whether a card is already active.
        /// </summary>
        public bool CanThrowCard
        {
            get
            {
                if (_cardManager == null)
                    return false;

                // Check if enough time has passed since last throw
                bool cooldownExpired = Time.time >= _lastCardThrowTime + CardThrowCooldown;

                // Check if no card is currently active
                bool noCardActive = !_cardManager.IsCardActive(this);

                return cooldownExpired && noCardActive;
            }
        }

        /// <summary>
        /// Initializes the player controller with required dependencies
        /// </summary>
        /// <param name="cardManager">The card manager service</param>
        public void Initialize(CardManager cardManager)
        {
            if (cardManager == null)
                throw new System.ArgumentNullException(nameof(cardManager), "PlayerController requires CardManager");

            _cardManager = cardManager;
        }

        /// <summary>
        /// Throws a card in the specified direction.
        /// Only throws if CanThrowCard is true (respects cooldown and active card).
        /// </summary>
        /// <param name="direction">The direction to throw the card</param>
        public void ThrowCard(Vector2 direction)
        {
            if (!CanThrowCard)
                return;

            // Calculate spawn position offset from player
            Vector2 startPos = (Vector2)transform.position + direction.normalized * 1f;

            // Create card via CardManager
            _cardManager.CreateCard(this, startPos, direction);

            // Update cooldown timer
            _lastCardThrowTime = Time.time;
        }

        /// <summary>
        /// Teleports the player to a safe position.
        /// Called by the card when false trigger is activated.
        /// </summary>
        /// <param name="cardTransform">The transform of the card (unused but required by interface)</param>
        /// <param name="safePosition">The safe position to teleport to</param>
        public void Teleport(Transform cardTransform, Vector2 safePosition)
        {
            // Teleport player to safe position
            transform.position = safePosition;

            // TODO: Add player-specific teleport effects (camera shake, particles, etc.)
            // These will be added in future when integrating with existing player systems
        }

        /// <summary>
        /// Called when the player's card is destroyed.
        /// Handles any player-specific cleanup or state updates.
        /// </summary>
        /// <param name="reason">The reason for the card's destruction</param>
        public void OnCardDestroyed(ICardManager.CardDestructionTypes reason)
        {
            // Card destruction is already handled by CardManager
            // This callback allows player to perform additional actions if needed

            // TODO: Add player-specific destruction handling (UI updates, etc.)
            // For now, no additional action needed - cooldown is handled by CanThrowCard
        }
    }
}

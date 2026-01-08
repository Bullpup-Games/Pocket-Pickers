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
        private float _lastCardThrowTime;
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
        /// Indicates whether the player can currently throw a card
        /// </summary>
        public bool CanThrowCard
        {
            get
            {
                throw new System.NotImplementedException("Task 4.2: Implement CanThrowCard property");
            }
        }

        /// <summary>
        /// Initializes the player controller with required dependencies
        /// </summary>
        /// <param name="cardManager">The card manager service</param>
        public void Initialize(CardManager cardManager)
        {
            throw new System.NotImplementedException("Task 4.2: Implement Initialize method");
        }

        /// <summary>
        /// Throws a card in the specified direction
        /// </summary>
        /// <param name="direction">The direction to throw the card</param>
        public void ThrowCard(Vector2 direction)
        {
            throw new System.NotImplementedException("Task 4.2: Implement ThrowCard method");
        }

        /// <summary>
        /// Teleports the player to a safe position
        /// </summary>
        /// <param name="cardTransform">The transform of the card</param>
        /// <param name="safePosition">The safe position to teleport to</param>
        public void Teleport(Transform cardTransform, Vector2 safePosition)
        {
            throw new System.NotImplementedException("Task 4.2: Implement Teleport method");
        }

        /// <summary>
        /// Called when the player's card is destroyed
        /// </summary>
        /// <param name="reason">The reason for the card's destruction</param>
        public void OnCardDestroyed(ICardManager.CardDestructionTypes reason)
        {
            throw new System.NotImplementedException("Task 4.2: Implement OnCardDestroyed method");
        }
    }
}

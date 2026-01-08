using System;
using System.Collections.Generic;
using _Scripts.Card;
using UnityEngine;

/// <summary>
/// Manages the lifecycle of all cards in play, tracking ownership and handling creation/destruction.
/// Supports multiple card owners (player, enemies) with individual card instances per owner.
/// </summary>
public class CardManager : MonoBehaviour, ICardManager
{
    // === DEPENDENCIES (injected) ===
    private CardEffectHandler _effectHandler;

    // === CARD TRACKING (multi-instance support) ===
    private Dictionary<ICardOwner, Card> _activeCards = new Dictionary<ICardOwner, Card>();
    private Dictionary<Card, ICardOwner> _cardToOwner = new Dictionary<Card, ICardOwner>();

    // === EXISTING FIELDS (preserved for Card.cs compatibility) ===
    [SerializeField] private GameObject cardPrefab;
    public float cardLifeTime = 5f; // Referenced by Card.cs for lifetime checks

    // === INITIALIZATION ===

    /// <summary>
    /// Initializes the CardManager with required dependencies
    /// </summary>
    /// <param name="effects">The card effect handler for particle effects</param>
    /// <exception cref="ArgumentNullException">Thrown when effects is null</exception>
    public void Initialize(CardEffectHandler effects)
    {
        if (effects == null)
            throw new ArgumentNullException(nameof(effects), "CardManager requires CardEffectHandler");

        _effectHandler = effects;
    }

    // === LIFECYCLE MANAGEMENT ===

    /// <summary>
    /// Instantiates a card object belonging to an ICardOwner
    /// </summary>
    /// <param name="owner">The owner of the card being created</param>
    /// <param name="startPos">The starting position for the card</param>
    /// <param name="direction">The direction the card should travel</param>
    public void CreateCard(ICardOwner owner, Vector2 startPos, Vector2 direction)
    {
        // Validation
        if (owner == null)
        {
            Debug.LogError("CardManager.CreateCard: owner cannot be null");
            return;
        }

        if (_activeCards.ContainsKey(owner))
        {
            Debug.LogWarning($"Owner {owner} already has active card");
            return;
        }

        // Instantiate card from prefab
        GameObject cardObject = Instantiate(cardPrefab, startPos, Quaternion.identity);
        Card card = cardObject.GetComponent<Card>();

        if (card == null)
        {
            Debug.LogError("CardManager.CreateCard: cardPrefab does not have Card component");
            Destroy(cardObject);
            return;
        }

        // Initialize card with owner and services
        card.Initialize(owner, this, _effectHandler);

        // Track card
        _activeCards[owner] = card;
        _cardToOwner[card] = owner;

        // Launch card with specified direction
        card.Launch(direction);
    }

    /// <summary>
    /// Destroys the card owned by the specified owner with a particle effect
    /// </summary>
    /// <param name="cardOwner">The owner whose card should be destroyed</param>
    /// <param name="particleEffect">The type of destruction effect to play</param>
    public void DestroyCard(ICardOwner cardOwner, ICardManager.CardDestructionTypes particleEffect)
    {
        if (cardOwner == null)
        {
            return; // Silently ignore null owner destruction
        }

        if (!_activeCards.TryGetValue(cardOwner, out Card card))
        {
            return; // No card to destroy
        }

        // Cleanup tracking before destruction
        _activeCards.Remove(cardOwner);
        _cardToOwner.Remove(card);

        // Notify owner of card destruction
        cardOwner.OnCardDestroyed(particleEffect);

        // Destroy card GameObject
        if (card != null)
        {
            Destroy(card.gameObject);
        }
    }

    /// <summary>
    /// Checks if the specified owner has an active card
    /// </summary>
    /// <param name="owner">The owner to check for an active card</param>
    /// <returns>True if the owner has an active card, false otherwise</returns>
    public bool IsCardActive(ICardOwner owner)
    {
        if (owner == null)
            return false;

        return _activeCards.ContainsKey(owner);
    }

    /// <summary>
    /// Gets the active card for the specified owner
    /// </summary>
    /// <param name="owner">The owner whose card to retrieve</param>
    /// <returns>The active card instance, or null if no card is active</returns>
    public Card GetCard(ICardOwner owner)
    {
        if (owner == null)
            return null;

        return _activeCards.TryGetValue(owner, out Card card) ? card : null;
    }

    // === INTERNAL METHODS ===

    /// <summary>
    /// Cleans up card tracking when owner is destroyed or scene is unloaded.
    /// Called by Card during OnDestroy to prevent dangling references.
    /// </summary>
    /// <param name="owner">The owner whose card tracking should be cleaned up</param>
    public void CleanupCardForOwner(ICardOwner owner)
    {
        if (owner == null)
            return;

        if (_activeCards.TryGetValue(owner, out Card card))
        {
            _activeCards.Remove(owner);
            _cardToOwner.Remove(card);
        }
    }
}

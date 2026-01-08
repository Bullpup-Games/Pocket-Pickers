using System;
using _Scripts.Card;
using UnityEngine;

public interface IGameStateManager
{
    void EscapeLevel();
    void Die();
}

public interface ISinManager
{
    event Action SinChanged;
    void CollectSin(GameObject sin);
    void SpendSin(UInt16 weight);
    void DepositSin(UInt16 weight);
    void WithdrawSin(UInt16 weight);
    void ReleaseSin(UInt16 weight);
}

public interface IPlayerController
{
    Vector2 Position { get; }
    bool IsDead { get; }
    void TeleportTo(Vector2 position);
    event Action<int> OnHealthChanged;
}

public interface IInputService
{
    Vector2 MovementInput { get; }
    bool JumpHeld { get; }
    event Action OnDash;
    event Action OnJumpPressed;
}

#region Card
/// <summary>
/// Manages the lifecycle of all cards in play, tracking ownership and handling creation/destruction
/// </summary>
public interface ICardManager
{
    /// <summary>
    /// Type of particle effect played during card destruction
    /// </summary>
    enum CardDestructionTypes
    {
        Normal,
        Teleport,
        Cancel,
        FalseTrigger,
        HitEnemy
    }

    /// <summary>
    /// Instantiates a card object belonging to an ICardOwner
    /// </summary>
    /// <param name="owner">The owner of the card being created</param>
    /// <param name="startPos">The starting position for the card</param>
    /// <param name="direction">The direction the card should travel</param>
    void CreateCard(ICardOwner owner, Vector2 startPos, Vector2 direction);

    /// <summary>
    /// Destroys the card owned by the specified owner with a particle effect
    /// </summary>
    /// <param name="cardOwner">The owner whose card should be destroyed</param>
    /// <param name="particleEffect">The type of destruction effect to play</param>
    void DestroyCard(ICardOwner cardOwner, CardDestructionTypes particleEffect);

    /// <summary>
    /// Checks if the specified owner has an active card
    /// </summary>
    /// <param name="owner">The owner to check for an active card</param>
    /// <returns>True if the owner has an active card, false otherwise</returns>
    bool IsCardActive(ICardOwner owner);

    /// <summary>
    /// Gets the active card for the specified owner
    /// </summary>
    /// <param name="owner">The owner whose card to retrieve</param>
    /// <returns>The active card instance, or null if no card is active</returns>
    Card GetCard(ICardOwner owner);

    /// <summary>
    /// Initializes the CardManager with required dependencies
    /// </summary>
    /// <param name="effects">The card effect handler for particle effects</param>
    void Initialize(CardEffectHandler effects);
}

/// <summary>
/// Represents an entity that can own and throw cards (e.g., player, enemies)
/// </summary>
public interface ICardOwner
{
    /// <summary>
    /// Transform of the card owner for position reference and spawning
    /// </summary>
    Transform transform { get; set; }

    /// <summary>
    /// Called when the card triggers teleportation
    /// </summary>
    /// <param name="cardTransform">The transform of the card</param>
    /// <param name="safePosition">The safe position to teleport to</param>
    void Teleport(Transform cardTransform, Vector2 safePosition);

    /// <summary>
    /// Initiates a card throw in the specified direction
    /// </summary>
    /// <param name="direction">The direction to throw the card</param>
    void ThrowCard(Vector2 direction);

    /// <summary>
    /// Indicates whether the owner can currently throw a card (cooldown, existing card, etc.)
    /// </summary>
    bool CanThrowCard { get; }

    /// <summary>
    /// Called when the owner's card is destroyed
    /// </summary>
    /// <param name="reason">The reason for the card's destruction</param>
    void OnCardDestroyed(ICardManager.CardDestructionTypes reason);
}
#endregion
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

# region Card
// Single instance that handles the lifecycle of all cards in play
public interface ICardManager
{
    // Type of particle effect played during the card destruction
    enum CardDestructionTypes
    {
        Normal,
        Teleport,
        Cancel,
        FalseTrigger,
        HitEnemy
    }
    // Instantiates a card object belonging to an ICardOwner
    void CreateCard(ICardOwner owner, Vector2 startPos, Vector2 direction);
    // Destroys 
    void DestroyCard(ICardOwner cardOwner, CardDestructionTypes particleEffect);
}

public interface ICardOwner
{
    Transform transform { get; set; }
    void Teleport(Transform startPos, Vector2 endPos);
    void ThrowCard(Vector2 direction);
}
#endregion
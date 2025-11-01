using System;
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

public interface ICardService
{
    bool IsCardInScene();
    void ThrowCard(Vector2 direction);
    event Action<Vector2> OnTeleport;
}

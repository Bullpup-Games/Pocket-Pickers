# Card System Refactor Design Document

## Overview

This design document details the **ownership and lifecycle architecture refactor** for the card system. The refactor eliminates singleton dependencies and implements interface-based dependency injection while **preserving all existing gameplay code unchanged**.

**Critical Constraint**: The card's physics, movement, collision, bounce, and safe position tracking code is working perfectly and will NOT be modified. This refactor touches ONLY:
- Singleton pattern elimination (Card.Instance, CardManager.Instance)
- Dependency injection implementation
- Owner-to-card relationship management
- Input routing through owners instead of singletons

**Untouched Code**: MoveCard(), CheckForHit(), UpdateSafePosition(), CollideWithWall(), CollideWithEnemy(), all physics calculations, bounce logic, and safe position algorithm.

## Architecture

### High-Level Architecture

```mermaid
graph TB
    subgraph "Input Layer"
        Input[InputHandler Service]
    end

    subgraph "Owner Layer"
        Player[PlayerController<br/>implements ICardOwner]
        Enemy[EnemyController<br/>implements ICardOwner<br/>(Future)]
    end

    subgraph "Card Management"
        Manager[CardManager Service<br/>implements ICardManager]
        Card[Card MonoBehaviour<br/>EXISTING GAMEPLAY CODE]
    end

    subgraph "Support Services"
        Effects[CardEffectHandler Service]
        Audio[CardSoundEffectManager]
    end

    Input --> Player
    Input --> Enemy
    Player --> Manager
    Enemy --> Manager
    Manager -- "CreateCard(owner)" --> Card
    Card -- "gameplay code unchanged" --> Effects
    Card -- "gameplay code unchanged" --> Audio
    Card -- "stores reference" --> Player
    Card -- "stores reference" --> Enemy
```

### What Changes vs What Stays

**Changes (Ownership/Lifecycle Only)**:
- ❌ Remove Card.Instance singleton pattern
- ❌ Remove CardManager.Instance singleton pattern
- ✅ Add Card.Initialize(ICardOwner, services) method
- ✅ Add CardManager owner-to-card Dictionary tracking
- ✅ Replace PlayerVariables.Instance calls with injected ICardOwner reference
- ✅ Route input through owner instead of direct to card

**Stays Unchanged (All Gameplay)**:
- ✅ Card.MoveCard() - exact same code
- ✅ Card.CheckForHit() - exact same code
- ✅ Card.UpdateSafePosition() - exact same code
- ✅ Card.CollideWithWall() - exact same code
- ✅ Card.CollideWithEnemy() - exact same code
- ✅ All serialized fields (speed, totalBounces, etc.)
- ✅ All physics calculations
- ✅ All collision detection logic
- ✅ All bounce calculations
- ✅ All safe position tracking

## Components and Interfaces

### Core Interfaces

#### ICardManager (extend existing in Interfaces.cs)
```csharp
public interface ICardManager
{
    enum CardDestructionTypes
    {
        Normal,
        Teleport,
        Cancel,
        FalseTrigger,
        HitEnemy
    }

    // Create card owned by specific owner
    void CreateCard(ICardOwner owner, Vector2 startPos, Vector2 direction);

    // Destroy card owned by specific owner
    void DestroyCard(ICardOwner cardOwner, CardDestructionTypes particleEffect);

    // Check if owner has active card
    bool IsCardActive(ICardOwner owner);

    // Get active card for owner (null if none)
    Card GetCard(ICardOwner owner);

    // Initialize service with dependencies
    void Initialize(CardEffectHandler effects);
}
```

#### ICardOwner (extend existing in Interfaces.cs)
```csharp
public interface ICardOwner
{
    // Transform for position reference and spawning
    Transform transform { get; }

    // Called when card triggers teleportation
    void Teleport(Transform cardTransform, Vector2 safePosition);

    // Initiate card throw
    void ThrowCard(Vector2 direction);

    // Check if can throw card (cooldown, existing card, etc.)
    bool CanThrowCard { get; }

    // Called when card is destroyed
    void OnCardDestroyed(ICardManager.CardDestructionTypes reason);
}
```

### Implementation Classes

#### CardManager Service (Ownership Tracking)
```csharp
public class CardManager : MonoBehaviour, ICardManager
{
    // === DEPENDENCIES (injected) ===
    private CardEffectHandler _effectHandler;

    // === CARD TRACKING (new multi-instance support) ===
    private Dictionary<ICardOwner, Card> _activeCards = new Dictionary<ICardOwner, Card>();
    private Dictionary<Card, ICardOwner> _cardToOwner = new Dictionary<Card, ICardOwner>();

    // === EXISTING FIELDS (preserved) ===
    [SerializeField] private GameObject cardPrefab;
    public float cardLifeTime = 5f; // Preserved for existing Card.cs reference

    // === INITIALIZATION ===
    public void Initialize(CardEffectHandler effects)
    {
        _effectHandler = effects;
    }

    // === LIFECYCLE MANAGEMENT ===
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

        // Instantiate card (existing prefab)
        GameObject cardObject = Instantiate(cardPrefab, startPos, Quaternion.identity);
        Card card = cardObject.GetComponent<Card>();

        // Initialize card with owner and services
        card.Initialize(owner, this, _effectHandler);

        // Track card
        _activeCards[owner] = card;
        _cardToOwner[card] = owner;

        // Launch card (calls existing Launch method)
        card.Launch(direction);
    }

    public void DestroyCard(ICardOwner owner, ICardManager.CardDestructionTypes particleEffect)
    {
        if (!_activeCards.TryGetValue(owner, out Card card))
        {
            return; // No card to destroy
        }

        // Cleanup tracking
        _activeCards.Remove(owner);
        _cardToOwner.Remove(card);

        // Notify owner
        owner.OnCardDestroyed(particleEffect);

        // Destroy card GameObject
        Destroy(card.gameObject);
    }

    public bool IsCardActive(ICardOwner owner) => _activeCards.ContainsKey(owner);

    public Card GetCard(ICardOwner owner) => _activeCards.TryGetValue(owner, out Card card) ? card : null;

    // === INTERNAL METHODS ===
    // Called by Card when owner destroyed or scene unloaded
    public void CleanupCardForOwner(ICardOwner owner)
    {
        if (_activeCards.TryGetValue(owner, out Card card))
        {
            _activeCards.Remove(owner);
            _cardToOwner.Remove(card);
        }
    }
}
```

#### Card MonoBehaviour (Minimal Changes)
```csharp
namespace _Scripts.Card
{
    public class Card : MonoBehaviour
    {
        // === EXISTING SERIALIZED FIELDS (unchanged) ===
        [Header("False Trigger Settings")]
        [SerializeField] private float falseTriggerRadius = 4f;
        [SerializeField] private Color gizmoColor = Color.cyan;
        public float speed = 20f;
        public int totalBounces;
        public int bounces;
        public CardEffectHandler effectHandler;

        // === EXISTING PRIVATE FIELDS (unchanged) ===
        private Vector2 _direction;
        private Vector2 _velocity;
        private float _startTime;
        private Rigidbody2D _rb;
        private Vector2 _previousPosition;
        public Vector2 lastSafePosition;
        private const float MinMoveDistance = 0.01f;

        // === NEW: INJECTED DEPENDENCIES ===
        private ICardOwner _owner; // Replaces PlayerVariables.Instance usage
        private CardManager _cardManager; // Replaces CardManager.Instance usage

        // === REMOVE: SINGLETON PATTERN ===
        // DELETE THIS ENTIRE REGION:
        // #region Singleton
        //     public static Card Instance { get; set; }
        //     private static Card _instance;
        // #endregion

        // === NEW: INITIALIZATION METHOD ===
        public void Initialize(ICardOwner owner, CardManager cardManager, CardEffectHandler effects)
        {
            _owner = owner;
            _cardManager = cardManager;
            effectHandler = effects;
        }

        // === EXISTING LIFECYCLE METHODS (minimal changes) ===
        private void OnEnable()
        {
            // CHANGED: Subscribe to input through owner's input handler instead of singleton
            // This will be handled by owner passing input to card
            // effectHandler reference set via Initialize, not singleton
        }

        private void OnDestroy()
        {
            // CHANGED: Unsubscribe from owner's input (if subscriptions exist)
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            bounces = 0;

            // CHANGED: Use injected owner reference instead of PlayerVariables.Instance
            var playerCollider = _owner.transform.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>());

            lastSafePosition = _owner.transform.position;
            _startTime = Time.time;

            // CHANGED: Get direction from owner (owner handles HandleCardStanceArrow interaction)
            // This will be set via Launch() called by CardManager
        }

        // === EXISTING GAMEPLAY METHODS (COMPLETELY UNCHANGED) ===

        public void Launch(Vector2 direction)
        {
            // EXISTING CODE - unchanged
            _direction = direction.normalized;
            CalculateVelocity(_direction);
        }

        private void CalculateVelocity(Vector2 direction)
        {
            // EXISTING CODE - unchanged
            _velocity = direction * speed;
        }

        private void Update()
        {
            // CHANGED: Use injected _cardManager instead of CardManager.Instance
            if (Time.time - _startTime >= _cardManager.cardLifeTime)
            {
                DestroyCard(); // Calls method below
            }
        }

        private void FixedUpdate()
        {
            // EXISTING CODE - completely unchanged
            MoveCard();
            UpdateSafePosition();
        }

        private void MoveCard()
        {
            // EXISTING CODE - COMPLETELY UNCHANGED
            // (entire method body stays exactly as-is)
        }

        private void CheckForHit(Vector2 movement, ref Vector2 newPosition)
        {
            // EXISTING CODE - COMPLETELY UNCHANGED
            // (entire method body stays exactly as-is)
        }

        private void UpdateSafePosition()
        {
            // EXISTING CODE - COMPLETELY UNCHANGED
            // (entire method body stays exactly as-is)
        }

        private void CollideWithWall(RaycastHit2D hit, ref Vector2 newPosition)
        {
            // EXISTING CODE - COMPLETELY UNCHANGED
            // (entire method body stays exactly as-is)
        }

        private void CollideWithEnemy(RaycastHit2D hit)
        {
            // EXISTING CODE - COMPLETELY UNCHANGED
            // (entire method body stays exactly as-is)
        }

        // === CHANGED: Input handling now via owner ===
        public void ActivateFalseTrigger()
        {
            // Called by owner when false trigger input received
            // Existing false trigger logic (teleportation)
            TriggerTeleportation();
        }

        public void CancelCardThrow()
        {
            // Called by owner when cancel input received
            DestroyCard();
        }

        private void TriggerTeleportation()
        {
            // Notify owner to teleport
            _owner.Teleport(transform, lastSafePosition);

            // Destroy with teleport effect
            DestroyCard();
        }

        private void DestroyCard()
        {
            // CHANGED: Call CardManager to handle destruction instead of destroying self
            _cardManager.DestroyCard(_owner, ICardManager.CardDestructionTypes.Normal);
        }

        // === EXISTING HELPER METHODS (unchanged) ===
        // Any remaining private methods stay exactly as they are
    }
}
```

#### PlayerController ICardOwner Implementation
```csharp
public class PlayerController : MonoBehaviour, ICardOwner
{
    // === DEPENDENCIES ===
    private CardManager _cardManager;
    private InputHandler _inputHandler;

    // === CARD STATE ===
    private float _lastCardThrowTime;
    private const float CardThrowCooldown = 0.5f;

    // === ICARD OWNER IMPLEMENTATION ===
    public bool CanThrowCard => Time.time >= _lastCardThrowTime + CardThrowCooldown
                                && !_cardManager.IsCardActive(this);

    public void Initialize(CardManager cardManager, InputHandler inputHandler)
    {
        _cardManager = cardManager;
        _inputHandler = inputHandler;

        // Subscribe to input events
        _inputHandler.OnCardThrow += HandleCardThrowInput;
        _inputHandler.OnFalseTrigger += HandleFalseTriggerInput;
        _inputHandler.OnCancelActiveCard += HandleCancelCardInput;
    }

    public void ThrowCard(Vector2 direction)
    {
        if (!CanThrowCard) return;

        // Get spawn position offset from player
        Vector2 startPos = (Vector2)transform.position + direction.normalized * 1f;

        _cardManager.CreateCard(this, startPos, direction);
        _lastCardThrowTime = Time.time;
    }

    public void Teleport(Transform cardTransform, Vector2 safePosition)
    {
        // Teleport player to safe position
        transform.position = safePosition;

        // Additional player-specific effects (camera shake, particles, etc.)
    }

    public void OnCardDestroyed(ICardManager.CardDestructionTypes reason)
    {
        // Handle card destruction (UI updates, cooldown, etc.)
    }

    // === INPUT HANDLERS ===
    private void HandleCardThrowInput(Vector2 direction)
    {
        ThrowCard(direction);
    }

    private void HandleFalseTriggerInput()
    {
        Card activeCard = _cardManager.GetCard(this);
        if (activeCard != null)
        {
            activeCard.ActivateFalseTrigger();
        }
    }

    private void HandleCancelCardInput()
    {
        Card activeCard = _cardManager.GetCard(this);
        if (activeCard != null)
        {
            activeCard.CancelCardThrow();
        }
    }
}
```

### Supporting Services

#### CardEffectHandler Service (Remove Singleton)
```csharp
public class CardEffectHandler : MonoBehaviour
{
    // === REMOVE SINGLETON ===
    // DELETE:
    // public static CardEffectHandler Instance { get; private set; }

    // === EXISTING FIELDS (unchanged) ===
    [SerializeField] private GameObject cardLaunchEffectPrefab;
    [SerializeField] private GameObject normalDestructionPrefab;
    [SerializeField] private GameObject teleportEffectPrefab;
    // ... etc (all existing fields stay)

    // === EXISTING METHODS (unchanged) ===
    public void PlayCardDestructionEffect(Vector2 position, ICardManager.CardDestructionTypes type)
    {
        // EXISTING CODE - unchanged
    }

    // All other existing methods stay exactly as they are
}
```

## Error Handling

### Missing Dependencies
```csharp
public void Initialize(ICardOwner owner, CardManager cardManager, CardEffectHandler effects)
{
    if (owner == null)
        throw new ArgumentNullException(nameof(owner), "Card requires ICardOwner");
    if (cardManager == null)
        throw new ArgumentNullException(nameof(cardManager), "Card requires CardManager");
    if (effects == null)
        throw new ArgumentNullException(nameof(effects), "Card requires CardEffectHandler");

    _owner = owner;
    _cardManager = cardManager;
    effectHandler = effects;
}
```

### Invalid Operations
```csharp
public void CreateCard(ICardOwner owner, Vector2 startPos, Vector2 direction)
{
    if (owner == null)
    {
        Debug.LogError("CardManager.CreateCard: owner cannot be null");
        return;
    }

    if (_activeCards.ContainsKey(owner))
    {
        Debug.LogWarning($"Owner already has active card. Ignoring.");
        return;
    }

    // Proceed...
}
```

## Testing Strategy

### Unit Tests (Ownership/Lifecycle Only)
```csharp
[TestFixture]
public class CardManagerTests
{
    [Test]
    public void CreateCard_WithValidOwner_TracksCardForOwner()
    {
        // Arrange
        var manager = CreateCardManager();
        var mockOwner = CreateMockOwner();

        // Act
        manager.CreateCard(mockOwner, Vector2.zero, Vector2.right);

        // Assert
        Assert.IsTrue(manager.IsCardActive(mockOwner));
        Assert.IsNotNull(manager.GetCard(mockOwner));
    }

    [Test]
    public void CreateCard_WhenCardAlreadyExists_DoesNotCreateDuplicate()
    {
        // Arrange
        var manager = CreateCardManager();
        var mockOwner = CreateMockOwner();
        manager.CreateCard(mockOwner, Vector2.zero, Vector2.right);

        // Act
        manager.CreateCard(mockOwner, Vector2.zero, Vector2.left);

        // Assert
        Assert.AreEqual(1, CountActiveCards(manager)); // Still only 1
    }
}
```

### Integration Tests (Gameplay Preservation)
```csharp
[UnityTest]
public IEnumerator Card_AfterRefactor_PhysicsBehaviorIdentical()
{
    // Arrange: Create card with new DI system
    var owner = CreateTestOwner();
    var manager = CreateCardManager();
    manager.CreateCard(owner, Vector2.zero, Vector2.right);

    // Act: Let card physics run
    yield return new WaitForSeconds(0.5f);

    // Assert: Verify movement matches expected physics
    var card = manager.GetCard(owner);
    Assert.IsNotNull(card);
    // Position, velocity checks to ensure physics unchanged
}
```

### Manual Playtesting Checklist
- [ ] Card throw feels identical
- [ ] Card bounces identically
- [ ] Safe position teleportation works identically
- [ ] Card-enemy collisions work identically
- [ ] No visual or audio differences
- [ ] No performance differences

## Migration Strategy

### Phase 1: Add DI Infrastructure (No Behavioral Changes)
1. Add Initialize method to Card.cs
2. Add private fields for _owner, _cardManager
3. Keep singleton properties functional (don't remove yet)
4. Test that existing gameplay still works

### Phase 2: Update CardManager to Track Owners
1. Implement Dictionary-based owner tracking in CardManager
2. Implement CreateCard/DestroyCard with owner parameter
3. Keep old singleton methods functional (parallel implementation)
4. Test multi-owner scenarios in isolation

### Phase 3: Implement ICardOwner on Player
1. Create or update PlayerController to implement ICardOwner
2. Route input through PlayerController instead of direct to CardManager
3. Initialize Card with player as owner
4. Test player card throwing

### Phase 4: Switch Card to Use Injected References
1. Update Card.Awake to use _owner.transform instead of PlayerVariables.Instance
2. Update Card.Update to use _cardManager instead of CardManager.Instance
3. Remove Card.Instance singleton property
4. Test that gameplay is identical

### Phase 5: Remove Old Singleton Code
1. Delete CardManager.Instance property
2. Delete Card.Instance property
3. Update all scenes to use new system
4. Final regression testing

## Performance Considerations

**No Changes Expected**: Since we're only changing architecture (not algorithms), performance should be identical or slightly better (fewer FindObjectOfType calls).

**Monitoring**:
- Frame time should be identical pre/post refactor
- Memory allocations should be identical or lower
- No new GC pressure from Dictionary lookups (O(1) operations)

## Assumptions and Dependencies

### Technical Assumptions
- **Card gameplay code is final** - MoveCard, CheckForHit, UpdateSafePosition are correct and unchanging
- **Serialized field values are correct** - speed, totalBounces, etc. should not be moved to ScriptableObject
- **Existing prefab configuration is correct** - no new configuration needed

### External Dependencies
- InputHandler exists and provides card input events
- CardEffectHandler exists and works
- Player has or can implement ICardOwner interface

### Risk Mitigation
- **Preserve gameplay**: Side-by-side testing, manual playtest verification
- **Gradual migration**: Parallel implementation, old code stays functional until proven
- **Rollback ready**: Can revert to singleton pattern if issues found

---

**Requirements Traceability**: This design addresses requirements in [requirements.md](requirements.md)
- Architecture Requirements: Singleton elimination, DI implementation
- Lifecycle Requirements: Owner-based card tracking
- Gameplay Preservation: All physics/collision/movement code unchanged
- Testing Requirements: Unit tests for ownership, manual tests for gameplay

**Review Status**: Updated for narrow scope (ownership/lifecycle only)

**Last Updated**: 2025-12-28 (Revised)

**Reviewers**: Development Team Lead

# Pocket Pickers - Codebase Refactoring Analysis

**Generated:** 2025-10-22
**Analyzed by:** Claude Code

---

## Executive Summary

This Unity C# codebase suffers from **severe architectural debt** that will significantly impede scalability and maintainability. The primary issues are pervasive Singleton abuse, tight coupling between systems, God objects managing multiple responsibilities, and inconsistent architectural patterns. While the state machine implementations show good design, they're undermined by the surrounding infrastructure.

**Critical Path:** Address Singleton dependencies first, then extract service layer, then modularize managers.

---

## Priority 1: Critical Issues (Blocking Scalability)

### 1.1 Singleton Antipattern Epidemic

**Severity:** 🔴 CRITICAL
**Impact:** Makes testing impossible, creates hidden dependencies, prevents concurrent systems, tight coupling
**Effort:** HIGH (2-3 weeks)
**Files Affected:** Nearly every major system

#### Problem Analysis

The codebase has **13+ Singleton instances** creating a tangled web of dependencies:

- `GameManager.Instance`
- `PlayerMovement.Instance`
- `PlayerVariables.Instance`
- `PlayerStateManager.Instance`
- `CardManager.Instance`
- `InputHandler.Instance`
- `SaveManager.Instance`
- `Lives.Instance`
- `CameraController.Instance`
- `LevelLoader.Instance`
- `Card.Instance`
- `HandleCardStanceArrow.Instance`
- Plus enemy state managers

**Example of coupling cascade:**
```csharp
// PlayerMovement.cs:66
CardManager.Instance.Teleport += TeleportTo;
InputHandler.Instance.OnCrouch += ToggleCrouching;
InputHandler.Instance.OnJumpPressed += HandleJumpPressed;

// PlayerMovement.cs:95-96
if (GameManager.Instance.isDead || PauseMenu.IsPaused)

// PlayerMovement.cs:143
if (PlayerVariables.Instance.Stats.SnapInput)

// Card.cs:315
CardManager.Instance.lastFalseTriggerPosition = transform.position;

// GuardStateManager.cs:199
Rigidbody2D.velocity = dir * (PlayerVariables.Instance.Stats.DashSpeed * 0.1f);
```

**Why this is catastrophic:**
1. **Initialization order bugs** - Singletons may not exist when accessed (see FindObjectOfType pattern)
2. **Scene management nightmare** - Singletons don't survive scene transitions properly
3. **Testing impossibility** - Cannot unit test without entire Unity scene
4. **Concurrency impossible** - Cannot have multiple game instances (multiplayer, split-screen)
5. **Hidden dependencies** - No explicit dependency graph visible
6. **Tight coupling** - Enemy AI knows about player stats directly

#### Refactoring Strategy

**Phase 1: Dependency Injection Foundation (Week 1)**

Create a service locator and central dependency container:

```csharp
public class ServiceLocator
{
    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();

    private Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return service as T;
        throw new Exception($"Service {typeof(T).Name} not registered");
    }

    public void Clear() => _services.Clear();
}

// Bootstrap class to initialize all services in correct order
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private CardManager _cardManager;
    // etc...

    private void Awake()
    {
        // Register services in dependency order
        ServiceLocator.Instance.Register(_inputHandler);
        ServiceLocator.Instance.Register(_gameManager);
        ServiceLocator.Instance.Register(_cardManager);
        // Initialize services after registration
        _gameManager.Initialize();
        _cardManager.Initialize();
    }
}
```

**Phase 2: Extract Interfaces (Week 1-2)**

Define contracts for all major systems:

```csharp
public interface IGameStateManager
{
    event Action SinChanged;
    int RemainingSin { get; }
    void CollectSin(GameObject sin);
    void ReleaseSin(int weight);
    void EscapeLevel();
    void Die();
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
```

**Phase 3: Refactor Systems One-by-One (Week 2-3)**

Start with least dependent systems first:

```csharp
// Old:
public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    public InputHandler inputHandler;

    private void OnEnable()
    {
        inputHandler.OnCardThrow += HandleCardAction;
    }
}

// New:
public class CardManager : MonoBehaviour, ICardService
{
    private IInputService _input;
    private IPlayerController _player;

    public void Initialize(IInputService input, IPlayerController player)
    {
        _input = input;
        _player = player;
        _input.OnCardThrow += HandleCardAction;
    }

    private void HandleCardAction()
    {
        if (_player.IsDead) return;
        // ... logic
    }
}
```

**Benefits:**
- Explicit dependencies visible in Initialize()
- Can inject mocks for testing
- Clear initialization order
- Scene-independent
- Can support multiple instances

---

### 1.2 God Object: GameManager

**Severity:** 🔴 CRITICAL
**Impact:** Single Point of Failure, violates Single Responsibility Principle, 400+ lines
**Effort:** MEDIUM (1-2 weeks)
**File:** [GameManager.cs](Assets/_Scripts/GameManager.cs)

#### Problem Analysis

GameManager has **9+ responsibilities**:

1. **Sin Management** - CollectSin, ReleaseSin, InstantiateSin, PotentialSinToSin
2. **Game State** - EscapeLevel, Die, checkForGameComplete
3. **Save System Integration** - Cleanup, PurgeSin
4. **Prefab Factory** - InstantiateSin, InstantiatePotentialSin
5. **UI References** - quicktimeEventPanel, pauseMenuDefaultButton, deathPanel
6. **Scene Management** - Escape/Die transitions
7. **Death Penalty Logic** - RedistributeSin
8. **Player Coupling** - Direct calls to PlayerVariables
9. **Win Condition Logic** - winThreshold checking

**Code smells:**
```csharp
// GameManager.cs doing too much:
public GameObject quicktimeEventPanel; // UI concern
public GameObject pauseMenuDefaultButton; // UI concern
public GameObject deathPanel; // UI concern
public GameObject smallSinPrefab; // Factory concern
private void RedistributeSin(int sinToDistribute) // Death penalty logic
public void InstantiateSin(int weight, Vector3 pos) // Object spawning
SaveManager.Instance.Cleanup(); // Save system
LevelLoader.Instance.LoadLevel(...); // Scene management
```

#### Refactoring Strategy

**Split into focused services:**

```csharp
// 1. Sin Management Service
public class SinManager : MonoBehaviour
{
    private List<GameObject> _activeSins = new List<GameObject>();
    private List<GameObject> _potentialSins = new List<GameObject>();

    public int RemainingWeight => CalculateRemainingWeight();
    public event Action<int> OnSinCollected;
    public event Action<int> OnSinReleased;

    public void CollectSin(GameObject sin)
    {
        int weight = sin.GetComponent<Sin>().weight;
        _activeSins.Remove(sin);
        OnSinCollected?.Invoke(weight);
    }

    public void ReleaseSin(int weight, Vector3 position)
    {
        // Spawn sin at location
        OnSinReleased?.Invoke(weight);
    }
}

// 2. Sin Factory (ScriptableObject-based)
[CreateAssetMenu(fileName = "SinFactory", menuName = "Game/Sin Factory")]
public class SinFactory : ScriptableObject
{
    [SerializeField] private GameObject _smallSinPrefab;
    [SerializeField] private GameObject _mediumSinPrefab;
    [SerializeField] private GameObject _largeSinPrefab;
    [SerializeField] private GameObject _grandSinPrefab;

    public GameObject CreateSin(int weight, Vector3 position)
    {
        GameObject prefab = weight switch
        {
            < 25 => _smallSinPrefab,
            < 40 => _mediumSinPrefab,
            < 60 => _largeSinPrefab,
            _ => _grandSinPrefab
        };

        GameObject sin = Instantiate(prefab, position, Quaternion.identity);
        sin.GetComponent<Sin>().weight = weight;
        return sin;
    }
}

// 3. Game State Manager (replaces GameManager)
public class GameStateManager : MonoBehaviour
{
    [SerializeField] private SinManager _sinManager;
    [SerializeField] private LevelCompletionService _completion;
    [SerializeField] private DeathHandler _deathHandler;

    private int _winThreshold;

    public void EscapeLevel()
    {
        if (_completion.CheckWinCondition(_sinManager.RemainingWeight))
            _completion.CompleteGame();
        else
            _completion.EscapeToHub();
    }

    public void HandleDeath()
    {
        _deathHandler.ProcessDeath(_sinManager.RemainingWeight);
    }
}

// 4. Death Handler
public class DeathHandler : MonoBehaviour
{
    [SerializeField] private SinFactory _sinFactory;
    [SerializeField] private int _deathPenalty;

    public void ProcessDeath(int remainingSin)
    {
        int totalSin = remainingSin + _deathPenalty;
        List<int> segments = SegmentSin(totalSin);

        foreach (int segment in segments)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            _sinFactory.CreateSin(segment, spawnPos);
        }
    }

    private List<int> SegmentSin(int totalSin)
    {
        // Redistribution logic
    }
}

// 5. UI Manager (handles all UI references)
public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _deathPanel;
    [SerializeField] private GameObject _qtePanel;

    public void ShowDeathPanel()
    {
        _deathPanel.SetActive(true);
        // Set default button focus
    }
}
```

**Benefits:**
- Each class has ONE responsibility
- Testable in isolation
- Clear dependencies
- Easier to modify/extend
- No 400-line monster classes

---

### 1.3 Cross-System Coupling

**Severity:** 🔴 CRITICAL
**Impact:** Enemies depend on Player internals, systems tightly bound
**Effort:** MEDIUM (1 week)
**Files:** GuardStateManager.cs, Card.cs, PlayerMovement.cs

#### Problem Analysis

**Enemies accessing Player internals:**
```csharp
// GuardStateManager.cs:199 - Enemy using player dash speed!
Rigidbody2D.velocity = dir * (PlayerVariables.Instance.Stats.DashSpeed * 0.1f);

// GuardStateManager.cs:136
PlayerVariables.Instance.currentHealth--;
Lives.Instance.updateHearts();
```

**Card system tightly coupled to everything:**
```csharp
// Card.cs:101 - Card knows about player collider
var playerCollider = PlayerVariables.Instance.gameObject.GetComponent<Collider2D>();

// Card.cs:104
lastSafePosition = PlayerVariables.Instance.transform.position;

// Card.cs:329-350 - Card knows about every enemy type
var guardStateManager = col.GetComponent<IEnemyStateManager<GuardStateManager>>();
var sniperStateManager = col.GetComponent<IEnemyStateManager<SniperStateManager>>();
var skreecherStateManager = col.GetComponent<IEnemyStateManager<SkreecherStateManager>>();
```

**Player movement coupled to managers:**
```csharp
// PlayerMovement.cs:66-67
CardManager.Instance.Teleport += TeleportTo;
InputHandler.Instance.OnCrouch += ToggleCrouching;

// PlayerMovement.cs:369
GameManager.Instance.EscapeLevel();
```

#### Refactoring Strategy

**Create abstraction layers and use interfaces:**

```csharp
// 1. Combat System Abstraction
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 knockbackDirection);
    bool IsDead { get; }
}

public interface IKillable
{
    void Kill();
    event Action<IKillable> OnKilled;
}

// 2. Implement on both Player and Enemies
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth;

    public bool IsDead => _currentHealth <= 0;
    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    public void TakeDamage(int amount, Vector2 knockbackDirection)
    {
        _currentHealth -= amount;
        OnHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
            OnDeath?.Invoke();
    }
}

public class GuardHealth : MonoBehaviour, IKillable
{
    public event Action<IKillable> OnKilled;

    public void Kill()
    {
        OnKilled?.Invoke(this);
        // Transition to disabled state
    }
}

// 3. Card uses interface instead of concrete types
public class Card : MonoBehaviour
{
    private void CollideWithEnemy(RaycastHit2D hit)
    {
        var killable = hit.collider.GetComponent<IKillable>();
        if (killable != null)
        {
            killable.Kill();
            DestroyCard();
            return;
        }
    }
}

// 4. Enemy movement decoupled from player stats
[CreateAssetMenu(menuName = "Enemy/Guard Settings")]
public class GuardMovementSettings : ScriptableObject
{
    public float dashSpeed = 8f; // Own value, not coupled to player
    public float aggroSpeed = 10f;
    public float patrolSpeed = 4f;
}

public class GuardStateManager : MonoBehaviour
{
    [SerializeField] private GuardMovementSettings _movementSettings;

    public void DashForward()
    {
        var dir = Settings.isFacingRight ? Vector2.right : Vector2.left;
        Rigidbody2D.velocity = dir * _movementSettings.dashSpeed;
    }
}

// 5. Player movement decoupled from managers
public class PlayerMovement : MonoBehaviour
{
    // Inject dependencies instead of pulling singletons
    [SerializeField] private PlayerHealth _health;

    private IInputService _input;
    private ICardService _cardService;

    public void Initialize(IInputService input, ICardService cardService)
    {
        _input = input;
        _cardService = cardService;

        _input.OnCrouch += ToggleCrouching;
        _cardService.OnTeleport += TeleportTo;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EscapeRout"))
        {
            OnEscapeReached?.Invoke(); // Event instead of direct call
        }
    }
}
```

**Benefits:**
- Systems communicate through interfaces
- Can swap implementations
- Enemies don't know about Player class
- Card system is generic
- Testable with mocks

---

## Priority 2: High Impact Issues

### 2.1 Inconsistent State Pattern Implementation

**Severity:** 🟡 HIGH
**Impact:** Hard to add new states, duplicated transition logic
**Effort:** MEDIUM (1 week)
**Files:** PlayerStateManager.cs, GuardStateManager.cs, etc.

#### Problem Analysis

**Good:** States use interface pattern
**Bad:** Transition logic is scattered and inconsistent

```csharp
// PlayerStateManager.cs:81-87 - Ad-hoc state blocks
if (CurrentState == newState) return;
if (CurrentState == StunnedState && newState == DashingState) return;
if (CurrentState == StunnedState && newState == WallState) return;
if (newState == DashingState && !PlayerVariables.Instance.isDashEnabled) return;
if (newState == WallState && !PlayerVariables.Instance.isWallClimbEnabled) return;

// GuardStateManager.cs:160-164 - Different transition rules
if (CurrentState == StunnedState && (newState != SearchingState && newState != DisabledState))
{
    Debug.Log("Tried to exit stunned to bad state");
    return;
}
```

#### Refactoring Strategy

**Create a proper state machine with transition validation:**

```csharp
public interface IState<T>
{
    void Enter(T context);
    void Update();
    void FixedUpdate();
    void Exit();
    bool CanTransitionTo(IState<T> newState);
}

public class StateMachine<TContext, TState> where TState : IState<TContext>
{
    private TContext _context;
    private TState _currentState;
    private TState _previousState;

    // Transition table for valid transitions
    private Dictionary<Type, HashSet<Type>> _allowedTransitions;

    public TState CurrentState => _currentState;
    public event Action<TState, TState> OnStateChanged;

    public StateMachine(TContext context)
    {
        _context = context;
        _allowedTransitions = new Dictionary<Type, HashSet<Type>>();
    }

    public void AllowTransition<TFrom, TTo>()
        where TFrom : TState
        where TTo : TState
    {
        Type fromType = typeof(TFrom);
        if (!_allowedTransitions.ContainsKey(fromType))
            _allowedTransitions[fromType] = new HashSet<Type>();

        _allowedTransitions[fromType].Add(typeof(TTo));
    }

    public bool TryTransition(TState newState)
    {
        if (_currentState == null)
        {
            SetState(newState);
            return true;
        }

        // Check state's internal transition logic
        if (!_currentState.CanTransitionTo(newState))
            return false;

        // Check transition table
        Type currentType = _currentState.GetType();
        Type newType = newState.GetType();

        if (_allowedTransitions.TryGetValue(currentType, out var allowed))
        {
            if (!allowed.Contains(newType))
            {
                Debug.LogWarning($"Transition from {currentType.Name} to {newType.Name} not allowed");
                return false;
            }
        }

        SetState(newState);
        return true;
    }

    private void SetState(TState newState)
    {
        _previousState = _currentState;
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(_context);
        OnStateChanged?.Invoke(_previousState, _currentState);
    }

    public void Update() => _currentState?.Update();
    public void FixedUpdate() => _currentState?.FixedUpdate();
}

// Usage:
public class PlayerStateManager : MonoBehaviour
{
    private StateMachine<PlayerContext, IPlayerState> _stateMachine;
    private PlayerContext _context;

    private void Awake()
    {
        _context = new PlayerContext(this);
        _stateMachine = new StateMachine<PlayerContext, IPlayerState>(_context);

        // Define allowed transitions
        _stateMachine.AllowTransition<FreeMovingState, DashingState>();
        _stateMachine.AllowTransition<FreeMovingState, WallState>();
        _stateMachine.AllowTransition<FreeMovingState, StunnedState>();
        _stateMachine.AllowTransition<DashingState, FreeMovingState>();
        _stateMachine.AllowTransition<StunnedState, FreeMovingState>(); // Only to free moving
        // etc...

        _stateMachine.TryTransition(new FreeMovingState());
    }

    public void RequestTransition(IPlayerState newState)
    {
        _stateMachine.TryTransition(newState);
    }
}

// States now validate their own conditions
public class DashingState : IPlayerState
{
    public bool CanTransitionTo(IPlayerState newState)
    {
        // Can only exit after dash completes
        return _dashComplete;
    }
}
```

**Benefits:**
- Centralized transition logic
- Easy to visualize state graph
- States self-validate
- Consistent across all state machines
- Can generate state diagrams from transition table

---

### 2.2 SaveManager Design Issues

**Severity:** 🟡 HIGH
**Impact:** Fragile save system, scene-dependent, couples to GameManager
**Effort:** MEDIUM (4-5 days)
**File:** [SaveManager.cs](Assets/_Scripts/SaveManager.cs)

#### Problem Analysis

```csharp
// SaveManager.cs:64 - Depends on GameManager being present
GameManager.Instance.AddSinsInSceneToActiveSins();

// SaveManager.cs:84-97 - Public method with confusing casing (DeleteSaveFile vs deleteSaveFile)
public void DeleteSaveFile() { ... }
public void deleteSaveFile() { ... } // Line 148 - duplicate!

// SaveManager.cs:153-164 - Cleanup does both save AND scene transition responsibility
public void Cleanup()
{
    SaveGameState();
    // TODO transition to a seperate scene passed in by the escape or die function
}

// SaveManager.cs:322-367 - Private nested classes for serialization
[Serializable]
private class SinData { ... }
[Serializable]
private class PlayerData { ... }
```

#### Refactoring Strategy

```csharp
// 1. Separate data models from save logic
[Serializable]
public class GameSaveData
{
    public List<SinSaveData> Sins;
    public List<Vector3> PotentialSinLocations;
    public PlayerSaveData Player;
    public string SaveTimestamp;
}

[Serializable]
public class SinSaveData
{
    public int Weight;
    public Vector3 Location;
}

[Serializable]
public class PlayerSaveData
{
    public int SinHeld;
    public int SinAccrued;
    public int SinThreshold;
    public int CurrentHealth;
}

// 2. Create pure save/load service
public class SaveService
{
    private readonly string _savePath;

    public SaveService(string savePath)
    {
        _savePath = savePath;
    }

    public void Save(GameSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Debug.Log($"Game saved to {_savePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
            throw;
        }
    }

    public GameSaveData Load()
    {
        try
        {
            if (!File.Exists(_savePath))
                return null;

            string json = File.ReadAllText(_savePath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            return null;
        }
    }

    public void Delete()
    {
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }

    public bool SaveExists() => File.Exists(_savePath);
}

// 3. Game state snapshot creator
public class GameStateSerializer
{
    private readonly SinManager _sinManager;
    private readonly PlayerHealth _playerHealth;
    private readonly PlayerVariables _playerVariables;

    public GameStateSerializer(SinManager sinManager, PlayerHealth playerHealth, PlayerVariables playerVariables)
    {
        _sinManager = sinManager;
        _playerHealth = playerHealth;
        _playerVariables = playerVariables;
    }

    public GameSaveData CreateSnapshot()
    {
        return new GameSaveData
        {
            Sins = _sinManager.GetActiveSins().Select(s => new SinSaveData
            {
                Weight = s.GetComponent<Sin>().weight,
                Location = s.transform.position
            }).ToList(),
            PotentialSinLocations = _sinManager.GetPotentialSinLocations(),
            Player = new PlayerSaveData
            {
                SinHeld = _playerVariables.sinHeld,
                SinAccrued = _playerVariables.sinAccrued,
                SinThreshold = _playerVariables.sinThreshold,
                CurrentHealth = _playerHealth.CurrentHealth
            },
            SaveTimestamp = DateTime.Now.ToString("o")
        };
    }

    public void ApplySnapshot(GameSaveData data)
    {
        // Restore sins
        foreach (var sinData in data.Sins)
        {
            _sinManager.SpawnSin(sinData.Weight, sinData.Location);
        }

        // Restore potential sin locations
        foreach (var location in data.PotentialSinLocations)
        {
            _sinManager.SpawnPotentialSin(location);
        }

        // Restore player
        _playerVariables.sinHeld = data.Player.SinHeld;
        _playerVariables.sinAccrued = data.Player.SinAccrued;
        _playerVariables.sinThreshold = data.Player.SinThreshold;
        _playerHealth.SetHealth(data.Player.CurrentHealth);
    }
}

// 4. High-level save manager coordinator
public class SaveManager : MonoBehaviour
{
    [SerializeField] private string _saveFileName = "save.json";

    private SaveService _saveService;
    private GameStateSerializer _serializer;

    public void Initialize(SinManager sinManager, PlayerHealth playerHealth, PlayerVariables playerVariables)
    {
        string savePath = Path.Combine(Application.persistentDataPath, _saveFileName);
        _saveService = new SaveService(savePath);
        _serializer = new GameStateSerializer(sinManager, playerHealth, playerVariables);
    }

    public void SaveGame()
    {
        GameSaveData snapshot = _serializer.CreateSnapshot();
        _saveService.Save(snapshot);
    }

    public void LoadGame()
    {
        GameSaveData data = _saveService.Load();
        if (data != null)
        {
            _serializer.ApplySnapshot(data);
        }
    }

    public void DeleteSave()
    {
        _saveService.Delete();
    }

    public bool HasSave() => _saveService.SaveExists();
}
```

**Benefits:**
- Separation of concerns: data models, serialization, file I/O
- Pure classes testable without Unity
- Not coupled to GameManager
- Clear API
- Can swap serialization format easily

---

### 2.3 Input System Architecture

**Severity:** 🟡 HIGH
**Impact:** Input logic scattered across multiple classes
**Effort:** LOW (2-3 days)
**File:** [InputHandler.cs](Assets/_Scripts/InputHandler.cs)

#### Problem Analysis

```csharp
// InputHandler.cs:99 - Business logic in input handler!
if (!PlayerStateManager.Instance.IsStunnedState() && !CardManager.Instance.IsCardInScene())

// PlayerMovement.cs:92-108 - Movement handles its own input blocking
if (GameManager.Instance.isDead || PauseMenu.IsPaused)
{
    _frameInput = new FrameInput();
    return;
}

// Mixed concerns: InputHandler knows about game state
```

#### Refactoring Strategy

```csharp
// 1. Pure input provider (no game logic)
public class InputProvider : MonoBehaviour
{
    private PlayerInputActions _inputActions;

    public Vector2 MovementInput { get; private set; }
    public Vector2 AimInput { get; private set; }
    public bool JumpHeld { get; private set; }

    public event Action OnJumpPressed;
    public event Action OnDashPressed;
    public event Action OnThrowPressed;
    public event Action OnCancelPressed;

    private void Update()
    {
        MovementInput = _inputActions.Player.Move.ReadValue<Vector2>();
        AimInput = _inputActions.Player.Aim.ReadValue<Vector2>();
        JumpHeld = _inputActions.Player.Jump.ReadValue<float>() > 0.5f;
    }

    // Just fire events, no business logic
}

// 2. Input processor applies game rules
public class InputProcessor
{
    private readonly InputProvider _provider;
    private readonly IGameStateManager _gameState;

    public Vector2 ProcessedMovement { get; private set; }
    public Vector2 ProcessedAim { get; private set; }

    public void Update()
    {
        // Apply game state rules
        if (_gameState.IsPaused || _gameState.IsPlayerDead)
        {
            ProcessedMovement = Vector2.zero;
            ProcessedAim = Vector2.zero;
            return;
        }

        ProcessedMovement = _provider.MovementInput;
        ProcessedAim = _provider.AimInput;
    }
}

// 3. Input context for different game states
public interface IInputContext
{
    bool AllowsMovement { get; }
    bool AllowsActions { get; }
    Vector2 ProcessMovement(Vector2 rawInput);
}

public class FreeMovementInputContext : IInputContext
{
    public bool AllowsMovement => true;
    public bool AllowsActions => true;

    public Vector2 ProcessMovement(Vector2 rawInput) => rawInput;
}

public class StunnedInputContext : IInputContext
{
    public bool AllowsMovement => false;
    public bool AllowsActions => false;

    public Vector2 ProcessMovement(Vector2 rawInput) => Vector2.zero;
}

public class WallJumpInputContext : IInputContext
{
    public bool AllowsMovement => true;
    public bool AllowsActions => true;

    public Vector2 ProcessMovement(Vector2 rawInput)
    {
        // Reduce control during wall jump apex
        return rawInput * 0.25f;
    }
}
```

**Benefits:**
- Input reading separate from game logic
- Easy to add input contexts for different states
- Can replay inputs for testing
- Clear input flow: Provider → Processor → Consumers

---

## Priority 3: Medium Impact Issues

### 3.1 PlayerVariables God Object

**Severity:** 🟠 MEDIUM
**Impact:** Mixed responsibilities, unclear ownership
**Effort:** LOW (2 days)
**File:** [PlayerVariables.cs](Assets/_Scripts/Player/PlayerVariables.cs)

#### Problem

Mixing runtime state, configuration, and game logic:

```csharp
public class PlayerVariables : MonoBehaviour
{
    public bool isFacingRight = true; // Runtime state
    public ScriptableStats Stats; // Configuration
    public bool isDashEnabled = true; // Feature flags
    public int sinHeld; // Game state
    public int currentHealth { get; set; } = 3; // Health

    public void CollectSin(int weight) { ... } // Game logic
    public void CommitSin(int weight) { ... } // Game logic
    public void FlipLocalScale() { ... } // Transform manipulation
}
```

#### Solution

```csharp
// 1. Player Configuration (ScriptableObject)
[CreateAssetMenu(menuName = "Player/Config")]
public class PlayerConfig : ScriptableObject
{
    public ScriptableStats MovementStats;
    public int MaxHealth = 3;
    public bool DashEnabled = true;
    public bool WallClimbEnabled = true;
}

// 2. Player State (runtime data)
public class PlayerState
{
    public bool IsFacingRight { get; set; } = true;
    public int CurrentHealth { get; set; }
    public int SinHeld { get; private set; }
    public int SinAccrued { get; private set; }

    public void AddSin(int amount) => SinHeld += amount;
    public void AccrueSin(int amount) => SinAccrued += amount;
}

// 3. Player Transform Handler
public class PlayerTransformController
{
    private Transform _transform;
    private bool _isFacingRight = true;

    public bool IsFacingRight => _isFacingRight;

    public void Flip()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 scale = _transform.localScale;
        scale.x *= -1;
        _transform.localScale = scale;
    }
}

// 4. Player root component
public class Player : MonoBehaviour
{
    [SerializeField] private PlayerConfig _config;

    public PlayerState State { get; private set; }
    public PlayerTransformController Transform { get; private set; }
    public PlayerConfig Config => _config;

    private void Awake()
    {
        State = new PlayerState { CurrentHealth = _config.MaxHealth };
        Transform = new PlayerTransformController(transform);
    }
}
```

---

### 3.2 Lives/Health System Duplication

**Severity:** 🟠 MEDIUM
**Impact:** Confusing health tracking, UI tightly coupled
**Effort:** LOW (1 day)
**Files:** [Lives.cs](Assets/_Scripts/Lives.cs), PlayerVariables.cs

#### Problem

```csharp
// PlayerVariables.cs
public int currentHealth { get; set; } = 3;

// Lives.cs
public int currentHearts;
public int availableHearts;

// Lives.cs:56 - Syncing from PlayerVariables
currentHearts = PlayerVariables.Instance.currentHealth;

// PlayerStateManager.cs:136 - Direct manipulation
PlayerVariables.Instance.currentHealth--;
Lives.Instance.updateHearts();
```

#### Solution

```csharp
// Single source of truth
public class HealthSystem
{
    private int _currentHealth;
    private int _maxHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;

    public HealthSystem(int maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth == 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}

// UI just listens
public class HealthUI : MonoBehaviour
{
    [SerializeField] private Life[] _heartIcons;

    public void Initialize(HealthSystem health)
    {
        health.OnHealthChanged += UpdateDisplay;
        UpdateDisplay(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateDisplay(int current, int max)
    {
        for (int i = 0; i < _heartIcons.Length; i++)
        {
            if (i < current)
                _heartIcons[i].setActive();
            else if (i < max)
                _heartIcons[i].setInjured();
            else
                _heartIcons[i].setLocked();
        }
    }
}
```

---

### 3.3 Magic Numbers and String Tags

**Severity:** 🟠 MEDIUM
**Impact:** Error-prone, hard to maintain
**Effort:** LOW (1 day)

#### Problem

```csharp
// Card.cs:166
LayerMask.GetMask("Environment","Enemy")

// Card.cs:325
LayerMask.GetMask("Enemy")

// PlayerMovement.cs:366
if (other.CompareTag("EscapeRout"))

// Card.cs:361
if (col.gameObject.CompareTag("Player"))

// GameManager.cs:88
potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
```

#### Solution

```csharp
// Constants class
public static class GameLayers
{
    public static readonly int Environment = LayerMask.NameToLayer("Environment");
    public static readonly int Enemy = LayerMask.NameToLayer("Enemy");
    public static readonly int Player = LayerMask.NameToLayer("Player");

    public static readonly LayerMask EnvironmentMask = LayerMask.GetMask("Environment");
    public static readonly LayerMask EnemyMask = LayerMask.GetMask("Enemy");
    public static readonly LayerMask PlayerMask = LayerMask.GetMask("Player");
    public static readonly LayerMask CollisionMask = EnvironmentMask | EnemyMask;
}

public static class GameTags
{
    public const string Player = "Player";
    public const string Sin = "Sin";
    public const string PotentialSin = "PotentialSin";
    public const string EscapeRoute = "EscapeRout"; // Fix typo later
    public const string Permeable = "permeable";
}

// Usage
if (col.gameObject.CompareTag(GameTags.Player)) { ... }
var hit = Physics2D.Raycast(origin, direction, distance, GameLayers.CollisionMask);
```

---

## Priority 4: Code Quality Issues

### 4.1 Naming Inconsistencies

**Severity:** 🟢 LOW
**Impact:** Readability, professionalism
**Effort:** LOW (2-3 hours)

#### Issues

- `EscapeRout` should be `EscapeRoute`
- `DeleteSaveFile()` vs `deleteSaveFile()` (both exist!)
- `updateHearts()` should be `UpdateHearts()`
- `setActive()` should be `SetActive()`
- `changeFov()` should be `ChangeFov()`

#### Solution

Run global find-and-replace, follow C# conventions:
- PascalCase for public methods
- camelCase for private fields
- Fix typos

---

### 4.2 Dead/Commented Code

**Severity:** 🟢 LOW
**Impact:** Confusion, clutter
**Effort:** LOW (1 hour)

#### Examples

```csharp
// GameManager.cs:123-126
// GameObject newPotentialSin = Instantiate(potentialSinPrefab, sin.transform.position, Quaternion.identity);
// potentialSins.Add(newPotentialSin);
// Debug.Log("Number of potential sins: " + potentialSins.Count);
// InstantiatePotentialSin(sin.transform.position);

// CardManager.cs:238-256 - Entire commented-out methods
// private void HandleEnterCardStance() { ... }

// PlayerVariables.cs:95-98
// public void Escape()
// {
//     GameManager.Instance.EscapeLevel();
// }
```

#### Solution

Delete all commented code (use version control to recover if needed).

---

### 4.3 Debug.Log Spam

**Severity:** 🟢 LOW
**Impact:** Console noise, performance
**Effort:** LOW (1 hour)

#### Problem

100+ Debug.Log calls in production code:

```csharp
Debug.Log("Number of sins: " +activeSins.Count);
Debug.Log("The total amount of sin in the game is " + remainingSin);
Debug.Log("Remaining sin " + remainingSin);
```

#### Solution

```csharp
// Create logging utility
public static class GameLog
{
    public static bool EnableDebugLogs = false;

    [Conditional("UNITY_EDITOR")]
    public static void Debug(string message)
    {
        if (EnableDebugLogs)
            UnityEngine.Debug.Log(message);
    }

    public static void Warning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    public static void Error(string message)
    {
        UnityEngine.Debug.LogError(message);
    }
}

// Usage
GameLog.Debug($"Sins collected: {count}"); // Only in editor
GameLog.Error($"Save file corrupt!"); // Always logged
```

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1-2)
1. ✅ Create ServiceLocator and interfaces
2. ✅ Extract InputProvider from InputHandler
3. ✅ Create HealthSystem to replace dual health tracking
4. ✅ Add constants for layers/tags

### Phase 2: Core Refactoring (Week 3-5)
1. ✅ Break apart GameManager into services
2. ✅ Refactor SaveManager
3. ✅ Implement proper StateMachine base
4. ✅ Decouple Player/Enemy interactions

### Phase 3: Architecture Cleanup (Week 6-7)
1. ✅ Replace all Singleton access with DI
2. ✅ Implement abstraction layers
3. ✅ Create factory patterns for spawning
4. ✅ Add event bus for cross-system communication

### Phase 4: Polish (Week 8)
1. ✅ Remove dead code
2. ✅ Fix naming conventions
3. ✅ Add proper logging
4. ✅ Write unit tests for core systems

---

## Testing Strategy

After refactoring, these systems should be unit testable:

```csharp
[Test]
public void HealthSystem_TakeDamage_TriggersDeathWhenZero()
{
    var health = new HealthSystem(3);
    bool deathTriggered = false;
    health.OnDeath += () => deathTriggered = true;

    health.TakeDamage(3);

    Assert.IsTrue(deathTriggered);
    Assert.AreEqual(0, health.CurrentHealth);
}

[Test]
public void SaveService_SaveAndLoad_PreservesData()
{
    var saveService = new SaveService("test_save.json");
    var originalData = new GameSaveData { /* ... */ };

    saveService.Save(originalData);
    var loadedData = saveService.Load();

    Assert.AreEqual(originalData.Player.SinHeld, loadedData.Player.SinHeld);
}

[Test]
public void StateMachine_InvalidTransition_ReturnsF false()
{
    var stateMachine = new StateMachine<PlayerContext, IPlayerState>();
    stateMachine.AllowTransition<IdleState, RunState>();
    stateMachine.TryTransition(new IdleState());

    bool result = stateMachine.TryTransition(new JumpState()); // Not allowed

    Assert.IsFalse(result);
}
```

---

## Critical Evaluation

### What's Already Good

1. **State Pattern Usage** - Player and enemy state machines use proper pattern
2. **ScriptableObjects for Data** - `ScriptableStats` is good approach
3. **Event-Driven Architecture Attempts** - Some events used (needs consistency)
4. **Layer-based Collision** - Good use of Unity physics layers

### Biggest Risks

1. **Refactoring without tests** - No unit tests means high regression risk
2. **Scene dependencies** - Many systems rely on specific scene setup
3. **Save file compatibility** - Refactoring will break existing saves
4. **Learning curve** - Team needs to understand DI and service architecture

### Success Metrics

- [ ] Can unit test game logic without Unity
- [ ] Can add new enemy type in < 1 hour
- [ ] Can add new player state in < 30 minutes
- [ ] Zero Singleton.Instance calls in game code
- [ ] Save/Load works across scenes
- [ ] Can run multiple game instances (for testing)

---

## Conclusion

This codebase needs **significant architectural refactoring** to be maintainable long-term. The Singleton epidemic is the root cause of most issues - fix that first, and everything else becomes easier.

**Recommended approach:**
1. Start with a **parallel implementation** of the service layer
2. Gradually **migrate systems** one at a time
3. Keep old code functional until new code is proven
4. **Test extensively** at each step

**Estimated total effort:** 6-8 weeks for full refactoring with one experienced developer.

The good news: The core game logic (state machines, movement physics) is solid. The problems are all architectural - fixable with disciplined refactoring.

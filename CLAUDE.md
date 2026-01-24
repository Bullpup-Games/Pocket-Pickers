# Pocket Pickers - Development Guidelines

## Project Overview

**Pocket Pickers** is a 2D stealth platformer built in Unity where players navigate levels, avoid enemies, and collect "sin" while using a throwable card mechanic for teleportation and enemy interactions.

**Engine**: Unity 2022.3 LTS  
**Language**: C#  
**Architecture**: Transitioning from singleton-heavy to dependency injection-based  
**Current Focus**: Singleton refactoring initiative (see [refactor.md](refactor.md))

---

## Critical Restrictions

### Scene Directory Access
**DO NOT** access, read, or modify any files in `Assets/_Scenes/` without **explicit user permission**. Scene files (.unity) are binary and should only be modified through the Unity Editor.

**Why**: Scene files contain complex serialized data that can be corrupted by text-based edits. All scene modifications must go through Unity Editor.

**Exception**: User may explicitly request scene analysis or modification - only proceed with clear permission.

Always read contents of ./.kiro/ and ./.claude/ for full project context

---

## Project-Specific Architecture Patterns

### State Machine Pattern
The project uses a **state machine pattern** for both player and enemy behavior. This is working well and should be preserved.

**Example State Managers:**
- `PlayerStateManager.cs`
- `GuardStateManager.cs`
- `SniperStateManager.cs`
- `SkreecherStateManager.cs`

**Key Pattern:**
```csharp
public interface IEnemyStateManager<T>
{
    void ChangeState(IEnemyState<T> newState);
    // ...
}
```

**Guideline**: When adding new player or enemy behaviors, follow the existing state pattern. Do NOT refactor these state machines unless explicitly requested.

### Card System (Core Gameplay Mechanic)
The card throwing system is a critical gameplay feature:
- Player throws a card that bounces off walls
- Card tracks safe teleport positions during flight
- Player can teleport to the card via "false trigger" input
- Card can hit and disable enemies

**CRITICAL**: The card's physics, movement, collision detection, bounce logic, and safe position tracking are **working perfectly**. These gameplay algorithms should NEVER be modified during refactoring. See `.kiro/specs/card-system-refactor/` for the current ownership/lifecycle refactor scope.

### Sin Management System
"Sin" is the collectible currency/resource in the game:
- Players collect sin throughout levels
- Sin has weight that affects player movement
- Sin can be deposited, withdrawn, spent, or released
- Game tracks sin thresholds for win conditions
- NOTE: The sin system will undergo a full refactor in the future. As of now, this is out of scope.

**Files:**
- `SinManager.cs`
- `GameManager.cs` (being refactored)

### Enemy AI Types
Three enemy types with different behaviors:
1. **Guard** - Patrols, chases on sight, can dash
2. **Sniper** - Ranged attacks, vision cones, patrol routes
3. **Skreecher** - Alert behavior, calls other enemies

**Guideline**: Enemy behaviors are state-driven and should use the IEnemyStateManager pattern.

---

## Unity-Specific Guidelines

### MonoBehaviour Best Practices
- **Initialization**: Use `Awake()` for internal setup, `Start()` for external dependencies
- **Dependency Injection**: Add `Initialize()` methods for passing dependencies (transitioning away from singletons)
- **Serialized Fields**: Use `[SerializeField] private` for inspector-exposed fields
- **Public Fields**: Avoid public fields; prefer properties or methods

### Unity Lifecycle Order
```
Awake() → OnEnable() → Start() → Update()/FixedUpdate() → OnDisable() → OnDestroy()
```

**Common Pattern in This Project:**
```csharp
private void Awake()
{
    // Component references, internal setup
    _rigidbody = GetComponent<Rigidbody2D>();
}

public void Initialize(IDependency dep1, IDependency dep2)
{
    // Dependency injection (called by bootstrapper)
    _dependency1 = dep1;
    _dependency2 = dep2;
}

private void Start()
{
    // Subscribe to events, finalize setup
}
```

### Physics and Raycasting
The project uses **Physics2D** extensively:
- Raycasting for collision detection (especially in Card.cs)
- LayerMask filtering for performance
- OverlapBox/OverlapCircle for area checks

**Layer Usage:**
- `Environment` - Walls, floors, static obstacles
- `Enemy` - Enemy colliders
- `Player` - Player collider

**Guideline**: Use `LayerMask.NameToLayer("LayerName")` and `LayerMask.GetMask("Layer1", "Layer2")` consistently.

### Input System
The project uses **Unity's Input System**:
- `InputHandler.cs` handles all input
- Currently singleton-based (being refactored to service)
- Events for input actions (OnJumpPressed, OnDash, OnCardThrow, etc.)

**Guideline**: When refactoring input, preserve all input events and event-driven architecture.

---

## Refactoring Initiative

### Current Status
The codebase is undergoing a **singleton elimination refactor**. See [refactor.md](refactor.md) for the full analysis.

**Singleton Epidemic:**
- 13+ singleton instances create tight coupling
- Initialization order bugs
- Testing impossibility
- Cannot support multiple instances (e.g., multiple enemies with cards)

**Refactoring Strategy:**
1. Create interfaces (ICardManager, ICardOwner, IPlayerController, etc.)
2. Implement dependency injection via Initialize() methods
3. Use ServiceLocator or direct references for dependency management
4. Parallel implementation (new code alongside old)
5. Switch over once tested
6. Delete old singleton code

**Active Refactors:**
- ✅ Card System Ownership/Lifecycle - **IN PROGRESS** (see `.kiro/specs/card-system-refactor/`)
- ⏳ GameManager → Services split (planned)
- ⏳ SaveManager redesign (planned)
- ⏳ PlayerVariables → PlayerController (planned)

### Refactoring Rules
1. **NEVER modify working gameplay code** during architecture refactoring
2. **Preserve serialized field values** (inspector configuration stays the same)
3. **Test at each step** - don't batch multiple refactors
4. **Manual playtest after gameplay-adjacent changes** (e.g., card system)
5. **Use parallel implementation** - old code stays functional until new code is proven

---

## Test Driven Development Principles
- **Tests First** ALWAYS create tests with expected behavior accounting for edge cases before writing any code.
- **Negative Tests** - Write tests that are expected to fail as well as ones that are expected to pass. It is not explicit that things that should fail will, and thus always need to be tested.
- **Don't Modify Tests** - Think very hard when writing tests. Treat it entirely as its own phase of the project that needs to be thought about and crafted. Once tests are written and finalized, they should never be changed. (Adding new tests later is fine, but should be treated as a last resort due to a missed edge case, not something to rely upon).
- **100% Coverage** - Always cover 100% of the codebase with tests. Every single function should have a test or multiple tests depending on the situation.

### Unity Test Framework
- **Test Assembly**: Create `.asmdef` files for test assemblies
- **Play Mode Tests**: Use `[UnityTest]` for tests requiring MonoBehaviour/scene
- **Edit Mode Tests**: Use `[Test]` for pure C# logic tests
- **Mocking**: Create mock implementations of interfaces for isolated testing

**Example Test Structure:**
```csharp
[TestFixture]
public class CardManagerTests
{
    private CardManager _cardManager;
    private Mock<ICardOwner> _mockOwner;

    [SetUp]
    public void Setup()
    {
        _mockOwner = new Mock<ICardOwner>();
        _cardManager = new GameObject().AddComponent<CardManager>();
        _cardManager.Initialize(/* dependencies */);
    }

    [Test]
    public void CreateCard_WithValidOwner_CreatesCardInstance()
    {
        // Arrange, Act, Assert
    }
}
```

---

## Kiro System - Adaptation of Amazon's Spec-Driven Development

This project uses an adaptation of Amazon's **Kiro System** for structured feature development. The original Kiro system has been adapted to work with Claude Code through templates and workflow guidance.

### Kiro Workflow (Amazon's 3-Phase Approach)
1. **Requirements** (`requirements.md`) - What needs to be built
2. **Design** (`design.md`) - How it will be built
3. **Tasks** (`tasks.md`) - Step-by-step implementation plan

### Directory Structure
- `.kiro/specs/{feature-name}/` - Individual feature specifications
- `.kiro/kiro-system-templates/` - Templates and documentation
	- `requirements_template.md` - Template for requirements
	- `design_template.md` - Template for technical design
	- `tasks_template.md` - Template for implementation tasks
	- `how_kiro_works.md` - Detailed Kiro documentation

### How Claude Code Should Work with Kiro

#### When Asked to Create New Features:
1. **Check for existing specs first**: Look in `.kiro/specs/` for any existing feature documentation
2. **Use templates**: Copy templates from `.kiro/kiro-system-templates/` when creating new specs
3. **Follow the 3-phase process**: Requirements → Design → Tasks → Implementation
4. **Require approval**: Each phase needs explicit user approval before proceeding

#### Template Usage:
- **Requirements**: Use `requirements_template.md` to create user stories and EARS acceptance criteria
- **Design**: Use `design_template.md` for technical architecture and component design
- **Tasks**: Use `tasks_template.md` to break down implementation into numbered, actionable tasks

#### During Implementation:
- **Reference requirements**: Always link tasks back to specific requirements
- **Work incrementally**: Implement tasks one at a time, not all at once
- **Validate against specs**: Ensure implementations match the design and requirements
- **Update documentation**: Keep specs updated if changes are needed

#### Key Behaviors:
- **Always suggest using Kiro** when user wants to build new features
- **Guide through templates** if user is unfamiliar with the process
- **Enforce the approval process** - don't skip phases
- **Maintain traceability** from requirements to code

---

## Claude / Codex Collaboration

You MUST use Codex as a code review agent during task development. This section describes how Claude can collaborate with OpenAI Codex for autonomous code review and iteration workflows.

### Overview

Codex is an AI code assistant available in this environment that can perform thorough code reviews. Claude can invoke Codex programmatically using its non-interactive `exec` mode, allowing for an autonomous write-review-fix cycle.

### How to Invoke Codex

#### Non-Interactive Mode (Required)

Codex requires a TTY for interactive mode, which isn't available when Claude runs commands. Use the `exec` subcommand instead:

```bash
codex exec "Your prompt here" 2>&1
```

**Important:** Always append `2>&1` to capture both stdout and stderr.

#### Running in Background

Codex reviews are thorough and can take **5-10 minutes**. Always run in background mode:

```bash
# Run with background: true and timeout: 600000 (10 minutes)
codex exec "Review the uncommitted changes in this repository. Provide a code review with any issues, suggestions, or concerns." 2>&1
```

**CRITICAL: Never kill a Codex process early.** Let it complete its full analysis.

### Code Review Workflow

#### Step 1: Initial Review

Ask Codex to review uncommitted changes:

```bash
codex exec "Review the uncommitted changes in this repository. Provide a code review with any issues, suggestions, or concerns." 2>&1
```

For focused reviews, specify the scope:

```bash
codex exec "Review the uncommitted changes in this repository. Focus on the files in Assets/_Scripts/Card/. Provide a code review with any issues, suggestions, or concerns." 2>&1
```

#### Step 2: Monitor Progress

Check the background process output periodically using `TaskOutput`. Codex will show:
- `thinking` blocks - its analysis process
- `exec` blocks - commands it runs to examine the code
- Final `codex` block - the actual review findings

#### Step 3: Analyze Findings

Codex findings typically include:
- **File path and line numbers** - e.g., `Assets/_Scripts/Card/Card.cs:126-133`
- **Issue description** - what's wrong
- **Suggested fix** - how to resolve it

#### Step 4: Implement Fixes

For each finding:
1. Read the relevant file section
2. Understand the issue
3. Implement the fix
4. Track progress in todo list

#### Step 5: Verify Fixes

Run Codex again to verify all issues are resolved:

```bash
codex exec "Review the uncommitted changes in this repository. Focus on [area]. Provide a code review with any issues, suggestions, or concerns." 2>&1
```

#### Step 6: Iterate Until Clean

Repeat Steps 3-5 until Codex finds no new bugs. Note that Codex may report:
- **Bugs** - actual issues that need fixing
- **Design suggestions** - valid observations that are intentional design choices

Use judgment to distinguish between bugs requiring fixes and suggestions for future consideration.

### Sample Prompts

#### General Code Review
```
Review the uncommitted changes in this repository. Provide a code review with any issues, suggestions, or concerns.
```

#### Focused Review
```
Review the uncommitted changes in this repository. Focus on the files in src/components/. Provide a code review with any issues, suggestions, or concerns.
```

#### Security Review
```
Review the uncommitted changes for security vulnerabilities. Focus on authentication, input validation, and data handling.
```

#### Architecture Review
```
Review the uncommitted changes for architectural concerns. Focus on separation of concerns, dependency management, and scalability.
```

### Best Practices

1. **Be patient** - Codex reviews take 5-10 minutes for thorough analysis
2. **Use todo lists** - Track each finding as a separate task
3. **Fix all bugs first** - Address actual bugs before considering design suggestions
4. **Verify with re-review** - Always run Codex again after making fixes
5. **Document decisions** - Note why design suggestions were accepted or declined
6. **Scope appropriately** - Focus reviews on specific areas when possible for faster iteration

### Interpreting Results

#### Bugs (Fix These)
- Wrong file paths or imports
- Logic errors (e.g., array index misalignment)
- Missing error handling that causes crashes
- Security vulnerabilities

#### Design Suggestions (Consider These)
- Portability concerns for internal tools
- Alternative approaches that are equally valid
- Intentional design decisions (e.g., empty content not getting vectors)

### Troubleshooting

#### "stdout is not a terminal"
Use `codex exec` instead of just `codex`.

#### Process seems stuck
Codex reviews are thorough. Wait the full 5-10 minutes before considering intervention.

#### No findings returned
Check that there are actually uncommitted changes (`git status`).

#### Codex can't find files
Ensure you're running from the correct working directory.

### Example Session

```
1. User: "Review my changes"
2. Claude: Runs `codex exec "Review uncommitted changes..." 2>&1` in background
3. Claude: Monitors with TaskOutput until complete
4. Codex: Reports 4 issues with file:line references
5. Claude: Creates todo list with 4 items
6. Claude: Fixes each issue, marking todos complete
7. Claude: Runs Codex review again
8. Codex: Reports no new bugs (only design suggestions)
9. Claude: Reports success to user
```

### Integration with Claude Workflow

This collaboration works best when:
- User requests a code review or asks Claude to fix issues
- Claude needs a "second opinion" on complex changes
- Validating that fixes are complete before committing
- Catching bugs that might be missed in manual review

The autonomous cycle (write → review → fix → verify) can run without user intervention, making it ideal for thorough code quality assurance.

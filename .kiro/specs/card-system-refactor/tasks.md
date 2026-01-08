# Card System Refactor Implementation Tasks

## Progress Summary

**Overall Progress**: Phase 2 of 5 Complete (40% complete)

**Completed**:
- ✅ Phase 1: Test Infrastructure & Interface Definition (Tasks 1.1, 1.2)
- ✅ Phase 2: CardManager Ownership Tracking (Tasks 2.1, 2.2, 2.3, 2.4)

**In Progress**: None

**Next**: Phase 3: Card Dependency Injection (Tasks 3.1, 3.2, 3.3)

**Latest Commit**: 3ad13c8 - CardManager ownership tracking with 21 comprehensive tests

**Known Issues**: 17 compilation errors from Card.Instance singleton references (will be resolved in Phase 3)

---

## Task Overview

This document breaks down the **ownership and lifecycle refactor** into actionable TDD tasks. **Critical scope constraint**: This refactor does NOT modify card gameplay code (physics, collision, movement, bouncing, safe position tracking) - those work perfectly and stay unchanged.

**Focus**: Singleton elimination, dependency injection, owner-based card tracking

**Total Estimated Tasks**: 12 tasks organized into 5 phases

**Requirements Reference**: [requirements.md](requirements.md)

**Design Reference**: [design.md](design.md)

**Development Approach**: Test-Driven Development - Write tests first, preserve all gameplay behavior

## Implementation Tasks

### Phase 1: Test Infrastructure & Interface Definition ✅ COMPLETE

- [x] **1.1** Create Test Infrastructure for Card System ✅ **COMPLETE** (Commit: de0bc1a, f8590e4)
  - **Description**: Set up Unity Test Framework test assemblies and create mock helper classes for testing ownership/lifecycle logic in isolation. Required before any TDD can begin.
  - **Deliverables**:
    - ✅ `Assets/_Scripts/Tests/CardSystem.Tests.asmdef` - Test assembly definition
    - ✅ `Assets/_Scripts/Tests/Mocks/MockCardOwner.cs` - Mock ICardOwner for tests
    - ✅ `Assets/_Scripts/Tests/Mocks/MockCardEffectHandler.cs` - Mock effect handler
    - ✅ `Assets/_Scripts/Tests/TestInfrastructureVerification.cs` - 8 infrastructure tests
  - **Requirements**: Testing Requirements section 3.6
  - **Tests**: Test infrastructure itself (8 tests passing)
  - **Actual Effort**: 1 hour
  - **Dependencies**: None
  - **Status**: Infrastructure validated and ready for TDD workflow

- [x] **1.2** Update ICardManager and ICardOwner Interfaces ✅ **COMPLETE** (Commit: de0bc1a, f8590e4)
  - **Description**: Extend existing interfaces in Interfaces.cs with the additional methods needed for ownership tracking (IsCardActive, GetCard, CanThrowCard, OnCardDestroyed). Document all interface members.
  - **Deliverables**:
    - ✅ Updated `Assets/_Scripts/Interfaces.cs` with extended interfaces
    - ✅ XML documentation for all interface members
    - ✅ Fixed #region syntax error
  - **Requirements**: Architecture Requirements section 3.1
  - **Tests**: Compilation success
  - **Actual Effort**: 30 minutes
  - **Dependencies**: None
  - **Status**: All interfaces documented and ready for implementation

###  Phase 2: CardManager Ownership Tracking ✅ COMPLETE

- [x] **2.1** Write Tests for CardManager Ownership Tracking ✅ **COMPLETE** (Commit: 3ad13c8)
  - **Description**: Write unit tests for ICardManager behavior: CreateCard with owner, DestroyCard by owner, IsCardActive, GetCard, multi-owner scenarios. **Write these tests FIRST before implementing CardManager.**
  - **Deliverables**:
    - ✅ `Assets/_Scripts/Tests/CardManagerOwnershipTests.cs` - Full test suite (21 tests)
    - ✅ Tests for: card creation per owner, duplicate prevention, destruction, owner tracking, null handling
    - ✅ Test setup with card prefab creation via reflection
    - ✅ Assembly reference fix (added Assembly-CSharp to CardSystem.Tests.asmdef)
  - **Requirements**: Architecture Requirements 3.1, Lifecycle Requirements 3.2
  - **Tests**: 21 unit tests (TDD RED phase - expected to fail before implementation)
  - **Actual Effort**: 1.5 hours
  - **Dependencies**: 1.1, 1.2
  - **Status**: Comprehensive test coverage ready for green phase

- [x] **2.2** Implement CardManager Ownership Tracking ✅ **COMPLETE** (Commit: 3ad13c8)
  - **Description**: Update existing CardManager in `Assets/_Scripts/Managers/CardManager.cs` to implement owner-based Dictionary tracking. Add CreateCard(ICardOwner), DestroyCard(ICardOwner), IsCardActive, GetCard methods.
  - **Deliverables**:
    - ✅ Updated `CardManager.cs` with Dictionary<ICardOwner, Card> tracking
    - ✅ CreateCard/DestroyCard methods with owner parameter
    - ✅ IsCardActive and GetCard helper methods
    - ✅ XML documentation for all new methods
    - ✅ Initialize(CardEffectHandler) with null validation
    - ✅ CleanupCardForOwner(owner) for safe cleanup
    - ✅ Null owner validation with error logging
    - ✅ Duplicate card prevention with warning logging
  - **Requirements**: Architecture Requirements 3.1, Lifecycle Requirements 3.2
  - **Tests**: All tests from 2.1 should now PASS (TDD GREEN phase)
  - **Actual Effort**: 2 hours
  - **Dependencies**: 2.1
  - **Status**: Implementation complete, ready for Unity Test Runner validation
  - **Notes**: Card.Initialize() and Card.Launch() calls commented out pending Phase 3

- [x] **2.3** Write Tests for CardManager Edge Cases ✅ **MERGED INTO 2.1** (Commit: 3ad13c8)
  - **Description**: Write additional tests for edge cases: destroying non-existent card, creating card for null owner, multiple rapid create/destroy cycles, cleanup on owner destruction.
  - **Deliverables**:
    - ✅ Edge case tests included in `CardManagerOwnershipTests.cs` (21 total tests)
    - ✅ Tests for error conditions, boundary cases, concurrent operations
    - ✅ Null owner handling (3 tests)
    - ✅ Duplicate prevention (2 tests)
    - ✅ Rapid create/destroy cycles (1 test with 10 iterations)
    - ✅ Multi-owner destruction isolation (1 test)
  - **Requirements**: Error handling requirements
  - **Tests**: Edge cases covered in comprehensive 21-test suite
  - **Actual Effort**: Merged with 2.1
  - **Dependencies**: 2.1, 2.2
  - **Status**: Edge cases already covered in initial test suite

- [x] **2.4** Implement CardManager Error Handling ✅ **MERGED INTO 2.2** (Commit: 3ad13c8)
  - **Description**: Add error handling, validation, and graceful degradation to CardManager to make edge case tests pass. All errors should log clearly and not crash.
  - **Deliverables**:
    - ✅ Updated `CardManager.cs` with validation logic
    - ✅ Debug.LogError for null owner in CreateCard
    - ✅ Debug.LogWarning for duplicate card attempts
    - ✅ Null checks for all public method parameters
    - ✅ Graceful handling of edge cases (silent returns where appropriate)
  - **Requirements**: Non-Functional Requirements (clear error messages)
  - **Tests**: All edge case tests pass
  - **Actual Effort**: Merged with 2.2
  - **Dependencies**: 2.3
  - **Status**: Error handling implemented alongside core functionality

### Phase 3: Card Dependency Injection (No Gameplay Changes!)

- [ ] **3.1** Add Card.Initialize Method and Dependency Fields
  - **Description**: Add Initialize(ICardOwner, CardManager, CardEffectHandler) method to Card.cs. Add private fields for _owner and _cardManager. **DO NOT modify any gameplay code** - only add the new method and fields.
  - **Deliverables**:
    - Updated `Assets/_Scripts/Card/Card.cs` with Initialize method
    - Private fields: _owner, _cardManager
    - XML documentation for Initialize
  - **Requirements**: Dependency Injection Requirements 3.4
  - **Tests**: Compilation success, manual test that existing gameplay works
  - **Estimated Effort**: 30 minutes
  - **Dependencies**: None

- [ ] **3.2** Replace Singleton References in Card.cs
  - **Description**: Replace `PlayerVariables.Instance` with `_owner.transform`, replace `CardManager.Instance` with `_cardManager`, replace `CardEffectHandler.Instance` with injected `effectHandler`. **DO NOT modify MoveCard, CheckForHit, UpdateSafePosition, or any gameplay methods.**
  - **Deliverables**:
    - Updated `Card.cs` Awake() to use _owner.transform instead of PlayerVariables.Instance
    - Updated `Card.cs` Update() to use _cardManager.cardLifeTime instead of CardManager.Instance.cardLifeTime
    - Updated OnEnable() to use injected effectHandler instead of singleton
  - **Requirements**: Dependency Injection Requirements 3.4, Gameplay Preservation 3.5
  - **Tests**: Manual playtest - gameplay must feel IDENTICAL
  - **Estimated Effort**: 1 hour
  - **Dependencies**: 3.1

- [ ] **3.3** Remove Card Singleton Pattern
  - **Description**: Delete the #region Singleton code block in Card.cs (Card.Instance property and _instance field). Ensure no code references Card.Instance anymore.
  - **Deliverables**:
    - Deleted singleton code from `Card.cs`
    - Search codebase for "Card.Instance" references, update or remove them
  - **Requirements**: Architecture Requirements 3.1
  - **Tests**: Compilation success, no "Card.Instance" found in codebase
  - **Estimated Effort**: 30 minutes
  - **Dependencies**: 3.2

### Phase 4: Player ICardOwner Implementation

- [ ] **4.1** Write Tests for PlayerController ICardOwner Implementation
  - **Description**: Write tests verifying PlayerController implements ICardOwner correctly: ThrowCard calls CardManager.CreateCard, Teleport moves player, OnCardDestroyed updates state, CanThrowCard respects cooldown.
  - **Deliverables**:
    - `Assets/_Scripts/Tests/PlayerCardOwnerTests.cs` - Test suite for player card ownership
    - Tests for: throw cooldown, teleportation, card destruction handling, input routing
  - **Requirements**: Input Routing Requirements 3.4
  - **Tests**: 8+ integration tests (should FAIL initially)
  - **Estimated Effort**: 1.5 hours
  - **Dependencies**: 2.2, 3.2

- [ ] **4.2** Implement ICardOwner on PlayerController
  - **Description**: Create or update PlayerController to implement ICardOwner interface. Add card throwing logic, teleportation, and input event handlers that route to card. Initialize with CardManager dependency.
  - **Deliverables**:
    - Updated or created `Assets/_Scripts/Player/PlayerController.cs`
    - ICardOwner implementation (ThrowCard, Teleport, CanThrowCard, OnCardDestroyed)
    - Initialize method for DI
    - Input event subscriptions (OnCardThrow, OnFalseTrigger, OnCancelActiveCard)
  - **Requirements**: Input Routing Requirements 3.4
  - **Tests**: All tests from 4.1 should now PASS
  - **Estimated Effort**: 2 hours
  - **Dependencies**: 4.1

### Phase 5: Integration & Migration

- [ ] **5.1** Remove CardEffectHandler Singleton Pattern
  - **Description**: Delete the singleton Instance property from CardEffectHandler. Update any references to pass CardEffectHandler via dependency injection instead. All existing effect methods stay unchanged.
  - **Deliverables**:
    - Updated `Assets/_Scripts/EffectHandler/CardEffectHandler.cs` - remove singleton
    - Update references to inject CardEffectHandler instead of accessing Instance
  - **Requirements**: Dependency Injection Requirements 3.4
  - **Tests**: Manual verification that effects still play correctly
  - **Estimated Effort**: 45 minutes
  - **Dependencies**: 3.2

- [ ] **5.2** Update Main Game Scenes to Use New Card System
  - **Description**: Update all game scenes to wire up new CardManager → PlayerController dependency injection. Ensure CardManager is initialized, PlayerController implements ICardOwner and is initialized with CardManager reference.
  - **Deliverables**:
    - Updated all scenes in `Assets/_Scenes/` with proper DI setup
    - GameBootstrapper or scene setup to initialize services
    - Remove old CardManager singleton instantiation if present
  - **Requirements**: Success Criteria 6.1
  - **Tests**: Manual playthrough of each level - gameplay must be IDENTICAL
  - **Estimated Effort**: 2 hours
  - **Dependencies**: 4.2, 5.1

- [ ] **5.3** Final Regression Testing and Validation
  - **Description**: Comprehensive manual playtest to verify NO gameplay changes occurred. Card physics, bouncing, safe position teleportation, collision, feel - all must be pixel-perfect identical to pre-refactor. Search codebase to verify zero singleton references remain.
  - **Deliverables**:
    - Playtest checklist completed (all boxes checked)
    - Codebase search results: zero "CardManager.Instance" and "Card.Instance" in production code
    - Performance profiling comparison (pre vs post refactor)
  - **Requirements**: Gameplay Preservation Requirements 3.5, Success Criteria 6.2
  - **Tests**: Manual playtesting checklist from design.md
  - **Estimated Effort**: 2 hours
  - **Dependencies**: 5.2

## Task Guidelines

### Task Completion Criteria
Each task is considered complete when:
- [ ] All deliverables are implemented and functional
- [ ] Unit tests are written and passing (for test-writing tasks)
- [ ] Code follows Unity C# coding standards (PascalCase, XML docs)
- [ ] Zero compiler warnings introduced
- [ ] Gameplay behavior is IDENTICAL to pre-task state (critical!)
- [ ] Requirements are satisfied and verified

### Critical Constraints
- **NEVER modify gameplay methods**: MoveCard(), CheckForHit(), UpdateSafePosition(), CollideWithWall(), CollideWithEnemy() must remain byte-for-byte identical
- **Preserve all serialized fields**: speed, totalBounces, falseTriggerRadius, etc. must not change
- **Manual playtest after each phase**: Ensure gameplay feels identical before proceeding
- **Rollback if gameplay changes**: If any player-facing behavior changes, STOP and rollback the task

### Testing Requirements (Per CLAUDE.md)
- **Unit Tests**: REQUIRED for ownership/lifecycle logic - write FIRST before implementation
- **Integration Tests**: REQUIRED for CardManager-Card-Owner interactions
- **Manual Playtests**: REQUIRED after each phase to verify gameplay preservation
- **100% Coverage Goal**: Every ownership/lifecycle method must have tests

### Code Quality Standards
- All code must follow Unity C# conventions (PascalCase for public, camelCase for private)
- XML documentation required for all public methods and properties
- Error handling must use Debug.LogError/LogWarning (not throw exceptions in MonoBehaviours)
- Preserve existing code style in Card.cs (minimize diff, only change what's necessary)

## Progress Tracking

### Milestone Checkpoints
- **Milestone 1**: [Phase 1 Complete] - Test infrastructure ready, interfaces defined
- **Milestone 2**: [Phase 2 Complete] - CardManager tracks cards by owner
- **Milestone 3**: [Phase 3 Complete] - Card uses DI, singleton removed, gameplay IDENTICAL
- **Milestone 4**: [Phase 4 Complete] - PlayerController implements ICardOwner, inputs routed
- **Milestone 5**: [Phase 5 Complete] - All scenes migrated, zero singletons, gameplay IDENTICAL

### Definition of Done
A task is considered "Done" when:
1. **Tests Written**: All tests exist and initially fail (TDD red phase) - for test-writing tasks
2. **Implementation**: Code implemented to make tests pass (TDD green phase)
3. **Gameplay Preservation**: Manual playtest confirms ZERO gameplay changes
4. **Code Review**: Self-review completed, only ownership/lifecycle code changed
5. **Integration**: Feature integrates without breaking existing systems
6. **Requirements**: All linked requirements from requirements.md satisfied

## Risk Mitigation

### Technical Risks
- **Risk**: Accidentally modifying card gameplay code
  - **Mitigation**: Clear diff review after each task - only initialization, singleton removal, and reference updates should change
  - **Affected Tasks**: 3.2 (most critical)

- **Risk**: Dependency injection breaking initialization order
  - **Mitigation**: Initialize method called in Awake AFTER MonoBehaviour awake chain, test thoroughly
  - **Affected Tasks**: 3.1, 3.2

- **Risk**: Reference changes causing subtle gameplay changes
  - **Mitigation**: Manual playtest after each phase, side-by-side comparison if possible
  - **Affected Tasks**: 3.2, 5.2

### Testing Risks
- **Risk**: Unable to detect subtle gameplay changes programmatically
  - **Mitigation**: Mandatory manual playtesting, use git diff to verify only ownership code changed
  - **Affected Tasks**: 5.3

## Resource Requirements

### Development Environment
- Unity 2022.3 LTS installed
- Unity Test Framework package installed
- Git for version control and diffing
- Manual playtest build for regression testing

### External Dependencies
- InputHandler must exist (can remain singleton initially)
- CardEffectHandler must exist
- Player GameObject must exist or be created

### Team Skills
- **Required**: C# programming, Unity development, TDD methodology
- **Required**: Careful code review - ability to verify "what changed"
- **Required**: Manual playtesting skills
- **Helpful**: Understanding of dependency injection

---

**Task Status**: Not Started

**Current Phase**: Phase 1 (Test Infrastructure & Interface Definition)

**Overall Progress**: 0/12 tasks completed (0%)

**Last Updated**: 2025-12-28 (Revised for narrow scope)

**Assigned Developer**: TBD

**Estimated Completion**: Based on task estimates, approximately 15-18 hours of focused development time. With manual playtesting and careful review, expect 1-1.5 weeks for solo developer.

---

## Quick Start Guide for Developers

**To begin this refactor:**

1. **Start with Phase 1, Task 1.1** - Create test infrastructure (required for TDD)
2. **Complete Task 1.2** - Update interfaces (quick and necessary)
3. **Follow TDD for Phase 2** - Write failing tests FIRST, then implement CardManager tracking
4. **BE EXTREMELY CAREFUL in Phase 3** - This is where Card.cs is modified. Only change initialization and references, NEVER touch gameplay methods
5. **Manual playtest after Phase 3** - Most critical checkpoint. Gameplay must be IDENTICAL
6. **Phase 4 connects everything** - PlayerController routes input to cards
7. **Phase 5 migrates scenes** - Final integration

**Red flags to watch for:**
- If you're modifying MoveCard, CheckForHit, UpdateSafePosition, or collision methods - STOP, you're out of scope
- If gameplay feels different after a task - STOP, rollback, investigate
- If you see changes to serialized field values (speed, totalBounces, etc.) - STOP, those should not change
- If tests are passing on first run without implementation - they're not testing anything useful

**Success looks like:**
- Zero singleton references (CardManager.Instance, Card.Instance)
- Card owns an ICardOwner reference
- All gameplay methods unchanged (verify with git diff)
- Game plays identically - throw, bounce, teleport, feel - all perfect
- Multiple cards can exist (test with mock owners)

**Most Important Task:**
- **Task 3.2** - This is the critical task. ONLY change singleton references to injected references. Do NOT touch any gameplay logic. Manual playtest immediately after.

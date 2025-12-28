# Card System Refactor Requirements

## 1. Introduction

This document specifies the requirements for refactoring the card system's **ownership and lifecycle architecture** from singleton-based to interface-driven with dependency injection. The card's existing gameplay logic (physics, movement, collision, bouncing, safe position tracking) is working perfectly and will **NOT be modified** during this refactor.

**Scope**: This refactor focuses exclusively on:
- Eliminating singleton dependencies (Card.Instance, CardManager.Instance)
- Implementing ICardManager and ICardOwner interfaces
- Enabling multiple card instances (player + future enemies)
- Dependency injection for services (InputHandler, CardEffectHandler, etc.)

**Out of Scope**: Card physics, collision detection, movement algorithms, bounce logic, safe position calculation - all remain unchanged.

**Architecture Overview**: Transform the card system from tightly-coupled singletons to an interface-driven architecture using ICardManager and ICardOwner (already defined in [Interfaces.cs](../../../Assets/_Scripts/Interfaces.cs)). This aligns with the broader refactoring initiative in [refactor.md](../../../.claude/refactor.md).

## 2. User Stories

### Developer (Team Member)
- **As a developer**, I want to add enemy card-throwing without code duplication, so that enemies can reuse the existing card gameplay logic
- **As a developer**, I want the card system to use dependency injection, so that I can test card lifecycle without Unity scenes
- **As a developer**, I want to eliminate singleton dependencies, so that the card system is modular and testable
- **As a developer**, I want clear interfaces between card owner and card manager, so that ownership logic is explicit

### Game Designer
- **As a game designer**, I want enemies to be able to throw cards, so that I can create enemy card-based encounters
- **As a game designer**, I want multiple cards in a scene simultaneously, so that player and enemies can have active cards at the same time
- **As a game designer**, I want existing card behavior preserved exactly, so that the refactor doesn't change gameplay feel

### QA/Testing
- **As a QA tester**, I want card ownership logic to be testable in isolation, so that I can verify multi-card scenarios without full level playthrough
- **As a QA tester**, I want the refactor to be transparent to gameplay, so that existing card behavior remains identical

## 3. Acceptance Criteria

### Architecture Requirements
- **WHEN** the refactor is complete, **THEN** the system **SHALL** have zero references to `CardManager.Instance` or `Card.Instance` in production code
- **WHEN** a card is created, **THEN** it **SHALL** receive owner reference and services through dependency injection, not singleton access
- **WHEN** implementing ICardOwner, **THEN** both player and future enemy classes **SHALL** use the same interface contract
- **IF** multiple ICardOwner instances exist in a scene, **THEN** each **SHALL** be able to create and manage their own card independently
- **WHEN** the refactor is complete, **THEN** all existing card gameplay logic (physics, collision, bouncing) **SHALL** remain byte-for-byte identical

### Card Lifecycle Requirements (Ownership Only)
- **WHEN** an ICardOwner throws a card, **THEN** ICardManager **SHALL** create a card instance associated with that specific owner
- **WHEN** a card is destroyed, **THEN** ICardManager **SHALL** notify the owning ICardOwner and clean up the card reference
- **WHEN** a card needs to reference its owner, **THEN** it **SHALL** use the injected ICardOwner reference, not PlayerVariables.Instance
- **IF** a card owner no longer exists, **THEN** the card **SHALL** be cleaned up automatically

### Input Routing Requirements
- **WHEN** input is received for card actions (throw, false trigger, cancel), **THEN** InputHandler **SHALL** notify the appropriate ICardOwner, not CardManager directly
- **WHEN** a card is in flight, **THEN** false trigger and cancel inputs **SHALL** route through the owning ICardOwner to the card instance
- **WHEN** multiple cards exist, **THEN** each input command **SHALL** affect only the card owned by the input recipient
- **IF** no active card exists for an owner, **THEN** input commands **SHALL** be ignored gracefully

### Dependency Injection Requirements
- **WHEN** Card is instantiated, **THEN** it **SHALL** receive ICardOwner, InputHandler, and CardEffectHandler through Initialize method
- **WHEN** CardManager is initialized, **THEN** it **SHALL** receive InputHandler and CardEffectHandler as constructor/initialize parameters
- **WHEN** the card needs to access player position, **THEN** it **SHALL** use ICardOwner.transform, not PlayerVariables.Instance
- **IF** a required dependency is missing, **THEN** the system **SHALL** fail gracefully with clear error messaging at initialization

### Gameplay Preservation Requirements (Critical!)
- **WHEN** the refactor is complete, **THEN** card movement speed, physics, and feel **SHALL** be identical to pre-refactor
- **WHEN** the card bounces off walls, **THEN** the bounce behavior **SHALL** be identical to pre-refactor
- **WHEN** the card tracks safe positions, **THEN** the safe position algorithm **SHALL** remain unchanged
- **WHEN** the card collides with enemies, **THEN** the collision detection **SHALL** work identically to pre-refactor
- **IF** any gameplay behavior changes, **THEN** the refactor **SHALL** be considered failed and rolled back

### Testing Requirements
- **WHEN** unit tests are written for ownership logic, **THEN** they **SHALL** NOT require MonoBehaviour or Unity scene setup
- **WHEN** integration tests verify card lifecycle, **THEN** mock ICardOwner implementations **SHALL** be usable
- **WHEN** testing multi-card scenarios, **THEN** multiple owners with cards **SHALL** be verifiable in test environment

## 4. Technical Architecture

### Unity Architecture
- **Engine**: Unity 2022.3 LTS (current project version)
- **State Management**: Interface-based dependency injection (ServiceLocator pattern from refactor.md)
- **Component Structure**: Existing Card MonoBehaviour with added Initialize method for DI
- **Configuration**: Existing serialized fields on Card and CardManager remain unchanged

### Core Systems
- **Input System**: Existing Unity Input System with InputHandler (refactor to service)
- **Physics**: Existing Physics2D with raycast-based collision detection (no changes)
- **Effects**: Existing CardEffectHandler (refactor from singleton to service)
- **Lifecycle**: New ICardManager implementation for owner-to-card tracking

### Key Dependencies
- **Interfaces**: ICardManager, ICardOwner (already defined in Interfaces.cs)
- **Input**: InputHandler (refactor from singleton to service)
- **Effects**: CardEffectHandler (refactor from singleton to service)
- **Existing Code**: Card.cs gameplay logic (preserved as-is)

## 5. Feature Specifications

### Core Features (Ownership & Lifecycle)
1. **Multi-Instance Card Tracking**: ICardManager tracks cards by owner (Dictionary<ICardOwner, Card>)
2. **Interface-Based Ownership**: ICardOwner contract implemented by player (and future enemies)
3. **Dependency Injection**: Services passed to Card and CardManager via Initialize methods
4. **Owner Reference**: Card stores ICardOwner reference instead of using PlayerVariables.Instance

### Refactoring Features (Architecture Only)
1. **Singleton Elimination**: Remove Card.Instance and CardManager.Instance pattern
2. **Event Decoupling**: Replace singleton event subscriptions with owner-mediated subscriptions
3. **Service Registration**: Register CardManager with ServiceLocator (or direct references initially)
4. **Reference Updates**: Update Card's player references to use ICardOwner.transform

### Preserved Features (Untouched Gameplay)
1. **Card Physics**: MoveCard, velocity calculations, gravity - all remain unchanged
2. **Collision Detection**: CheckForHit, CollideWithWall, CollideWithEnemy - all remain unchanged
3. **Safe Position Tracking**: UpdateSafePosition algorithm - remains unchanged
4. **Bounce Logic**: Bounce counting, reflection calculations - all remain unchanged
5. **Visual/Audio**: CardRotationController, HandleCardStanceArrow, effects - all remain unchanged

## 6. Success Criteria

### Architectural Success
- **WHEN** searching codebase for "CardManager.Instance", **THEN** zero results **SHALL** be found in production code
- **WHEN** searching codebase for "Card.Instance", **THEN** zero results **SHALL** be found in production code
- **WHEN** searching codebase for "PlayerVariables.Instance" in Card.cs, **THEN** zero results **SHALL** be found
- **WHEN** inspecting Card.cs, **THEN** all gameplay methods (MoveCard, CheckForHit, UpdateSafePosition) **SHALL** be unchanged

### Gameplay Success (Critical!)
- **WHEN** throwing the card, **THEN** it **SHALL** feel identical to pre-refactor (speed, arc, physics)
- **WHEN** the card bounces, **THEN** bounce behavior **SHALL** be pixel-perfect identical
- **WHEN** teleporting to card, **THEN** safe position calculation **SHALL** work exactly as before
- **WHEN** playtesting, **THEN** no player-facing changes **SHALL** be detectable

### Testing Success
- **WHEN** running unit tests, **THEN** card lifecycle and ownership **SHALL** be testable without Unity scene
- **WHEN** creating mock ICardOwner, **THEN** card creation and destruction **SHALL** be verifiable
- **WHEN** testing with multiple owners, **THEN** each owner's card **SHALL** track independently

### Future-Readiness Success
- **WHEN** implementing enemy card throwing, **THEN** adding ICardOwner to enemy **SHALL** enable card usage
- **WHEN** multiple enemies throw cards, **THEN** each card **SHALL** track its owner correctly
- **WHEN** designers need custom card behavior, **THEN** ICardOwner implementations **SHALL** allow per-owner customization

## 7. Assumptions and Dependencies

### Technical Assumptions
- Existing Card.cs gameplay code is correct and should not be modified
- Card physics, movement, and collision algorithms are final
- Safe position tracking algorithm is working correctly
- InputHandler, CardEffectHandler, and HandleCardStanceArrow exist and work

### External Dependencies
- ServiceLocator or similar DI container (can workaround with direct references initially)
- Input system is functional (InputHandler exists)
- Player has or can implement ICardOwner interface

### Resource Assumptions
- Existing card prefab works and only needs script updates
- Existing particle effects and audio work as-is
- No new assets or configurations needed

## 8. Constraints and Limitations

### Technical Constraints
- **MUST NOT** modify card physics, movement, or collision code
- **MUST NOT** change card gameplay feel or behavior
- **MUST** maintain 60fps performance (existing performance is acceptable)
- **MUST** preserve existing serialized field values on Card.cs

### Scope Constraints
- **IN SCOPE**: Ownership, lifecycle, singleton removal, dependency injection
- **OUT OF SCOPE**: Physics refactoring, collision algorithm changes, new gameplay features
- **OUT OF SCOPE**: UI changes, visual changes, audio changes

### Design Constraints
- ICardManager and ICardOwner interfaces already defined - use as-is or extend minimally
- Existing Card.cs public API (speed, totalBounces, lastSafePosition) should remain accessible
- Must support parallel implementation (new code alongside old during transition)

## 9. Risk Assessment

### Technical Risks
- **Risk**: Accidentally modifying card gameplay code during refactor
  - **Likelihood**: Medium
  - **Impact**: High (breaks working gameplay)
  - **Mitigation**: Clear scope definition, code review focused on "what changed", regression testing

- **Risk**: Dependency injection breaks existing initialization order
  - **Likelihood**: Low
  - **Impact**: Medium
  - **Mitigation**: Initialize method called in Awake, preserves Unity lifecycle order

- **Risk**: Reference changes break existing card behavior
  - **Likelihood**: Low
  - **Impact**: High
  - **Mitigation**: Only change singleton references to injected references, logic unchanged

### Integration Risks
- **Risk**: InputHandler refactor timing conflicts with card refactor
  - **Likelihood**: Low
  - **Impact**: Low
  - **Mitigation**: Can keep InputHandler as singleton initially, wrap with service layer

- **Risk**: PlayerVariables still used elsewhere during card refactor
  - **Likelihood**: High
  - **Impact**: None (card just needs ICardOwner.transform)
  - **Mitigation**: Player implements ICardOwner, provides transform property

### Testing Risks
- **Risk**: Unable to verify gameplay preservation programmatically
  - **Likelihood**: Medium
  - **Impact**: Medium
  - **Mitigation**: Manual playtesting required, side-by-side comparison pre/post refactor

## 10. Non-Functional Requirements

### Maintainability
- Code must follow Unity C# conventions (PascalCase public, camelCase private)
- Ownership logic must be clear and documented
- Dependency injection must be explicit and traceable

### Testability
- Card ownership logic must be testable without Unity scenes
- ICardManager must support mock ICardOwner implementations
- Multi-card scenarios must be testable in isolation

### Compatibility
- Existing card prefab must work without modification (just script updates)
- Existing scenes must work with minimal changes (add CardManager service)
- Existing card configuration (speed, bounces, etc.) must be preserved

### Performance
- No performance regression from pre-refactor baseline
- Multi-card support must maintain 60fps (test with 5 cards)
- No new memory allocations in hot paths

## 11. Future Considerations

### Phase 2 Features (Post-Refactor)
- Enemy card throwing implementation using ICardOwner
- Per-owner card configuration (different speeds, lifetimes, etc.)
- Card-on-card interactions (if multiple cards in scene)

### Integration Opportunities
- Card behavior modifications via ICardOwner (different physics per owner)
- Power-up system affecting card properties
- Multiplayer support (each player is ICardOwner)

### Technical Debt
- HandleCardStanceArrow singleton should eventually be refactored
- CardEffectHandler and CardSoundEffectManager singleton refactors
- InputHandler singleton to service migration

---

**Document Status**: Updated to scope gameplay preservation

**Last Updated**: 2025-12-28 (Revised)

**Stakeholders**: Development Team, Game Designers

**Related Documents**:
- [refactor.md](../../../.claude/refactor.md) - Overall refactoring strategy
- [Interfaces.cs](../../../Assets/_Scripts/Interfaces.cs) - Interface definitions
- [CLAUDE.md](../../../.claude/CLAUDE.md) - Development guidelines and TDD requirements
- [Card.cs](../../../Assets/_Scripts/Card/Card.cs) - Existing implementation (gameplay logic preserved)

**Version**: 2.0 (Narrow scope - ownership/lifecycle only)

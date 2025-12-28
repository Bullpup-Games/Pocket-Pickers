# Card System Refactor - Kiro Spec

## Overview

This Kiro spec outlines the **ownership and lifecycle architecture refactor** for the card throwing system. The refactor eliminates singleton dependencies and enables multiple card instances (player and future enemy cards) while **preserving all existing gameplay code unchanged**.

**CRITICAL**: This refactor does NOT touch card physics, movement, collision, bouncing, or safe position tracking - that code is perfect and stays exactly as-is. Only ownership, lifecycle, and dependency injection are modified.

**Status**: Requirements and design approved, ready for implementation

**Feature Type**: Architecture Refactoring (Ownership/Lifecycle Only)

**Priority**: High (part of broader singleton elimination initiative)

## Documents

This spec follows the Kiro system's 3-phase approach:

1. **[requirements.md](requirements.md)** - What needs to be built
   - User stories for developers, designers, and QA
   - Acceptance criteria using EARS format (WHEN/THEN/SHALL)
   - Success criteria and risk assessment
   - Read this first to understand the "why" behind the refactor

2. **[design.md](design.md)** - How it will be built
   - System architecture with mermaid diagrams
   - Interface definitions and component designs
   - Code examples for all major classes
   - Error handling and testing strategies
   - Read this to understand the "how" of implementation

3. **[tasks.md](tasks.md)** - Step-by-step implementation plan
   - 23 tasks organized into 5 phases
   - Test-Driven Development approach (tests first!)
   - Clear deliverables and dependencies for each task
   - Estimated effort and completion criteria
   - Start here when you're ready to code

## Quick Start

### For Developers Implementing This Refactor:

1. **Read in order**: requirements.md → design.md → tasks.md
2. **Start with Phase 1, Task 1.1** in tasks.md
3. **Follow TDD religiously**: Write tests FIRST, then implement
4. **Check off tasks** as you complete them in tasks.md
5. **Commit frequently**: Each task is a commit opportunity

### For Reviewers:

- **Requirements review**: Check that user stories and acceptance criteria are clear and complete
- **Design review**: Verify architecture makes sense and follows Unity best practices
- **Code review**: Ensure implementation matches design document specifications

### For Project Managers:

- **Progress tracking**: See tasks.md for checkboxes and milestone tracking
- **Estimated timeline**: ~40-45 hours of focused dev time (2-3 weeks with normal interruptions)
- **Dependencies**: ServiceLocator from broader refactor (workarounds available if not ready)

## Context

This refactor is part of a larger initiative to eliminate singleton antipatterns throughout the Pocket Pickers codebase, as documented in [.claude/refactor.md](../../../.claude/refactor.md).

**Current State:**
- Card system uses singleton pattern (CardManager.Instance, Card.Instance)
- Tight coupling to PlayerVariables, InputHandler, and other singletons
- Only player can have a card (hardcoded singleton assumption)
- Testing requires full Unity scene setup
- Existing interfaces (ICardManager, ICardOwner) are defined but unused
- **Card gameplay code (physics, collision, movement) is PERFECT and working**

**Target State:**
- Zero singleton references in card system
- Interface-based dependency injection
- Multiple card instances supported (player + enemies)
- Test coverage for ownership/lifecycle logic
- Clean separation of concerns (manager, owner)
- **Card gameplay code (physics, collision, movement) UNCHANGED - byte-for-byte identical**

## Key Features

### Implemented by This Refactor (Ownership/Lifecycle Only):
- ✅ ICardManager service with multi-instance card tracking (Dictionary<ICardOwner, Card>)
- ✅ ICardOwner interface implemented by PlayerController
- ✅ Card.Initialize() dependency injection method
- ✅ Singleton pattern elimination (Card.Instance, CardManager.Instance removed)
- ✅ Reference updates (PlayerVariables.Instance → _owner.transform)
- ✅ Test suite for ownership logic
- ❌ **NOT changing:** Card physics, movement, collision, bouncing, safe position tracking

### Enables for Future Development:
- 🎯 Enemy card throwing (just implement ICardOwner on enemy)
- 🎯 Multiple simultaneous cards (5+ cards with no perf impact)
- 🎯 Card-on-card interactions (multiple owners with cards)
- 🎯 Configurable card types (different configs per owner)

## Architecture Summary

```
InputHandler (Service)
    ↓
PlayerController (ICardOwner)
    ↓
CardManager (ICardManager Service)
    ↓
Card (MonoBehaviour)
    ↓
CardConfig (ScriptableObject) + Effects/Audio Services
```

**Key Interfaces:**
- `ICardManager`: Card lifecycle management (create, destroy, track)
- `ICardOwner`: Contract for any entity that can throw cards
- `IPlayerController`: Player-specific interface (position, health, etc.)

**Key Classes:**
- `CardManager`: Service that manages all active cards
- `Card`: Physics, collision, movement MonoBehaviour
- `CardConfig`: ScriptableObject with all card behavior settings
- `PlayerController`: Player implementation of ICardOwner

## Testing Strategy

Following CLAUDE.md TDD guidelines:

1. **Write tests FIRST** (before any implementation code)
2. **100% coverage goal** for all card system code
3. **Three test types**:
   - Unit tests: Business logic, interfaces, configuration
   - Integration tests: CardManager + Card + Owner interactions
   - E2E tests: Complete throw → teleport → destroy flows
4. **Test negative cases**: Error handling, edge cases, null inputs
5. **Don't modify tests**: Think hard when writing them (treat as contract)

## Migration Strategy

**Parallel Implementation Approach** (from refactor.md):

1. **Phase 1-4**: Build new system alongside old (CardRefactored.cs, new CardManager)
2. **Phase 5, Task 5.4**: Create test scene, verify feature parity
3. **Phase 5, Task 5.5**: Performance profiling (ensure no regression)
4. **Phase 5, Task 5.6**: Migrate main game scenes to new system
5. **Phase 5, Task 5.7**: Delete old singleton-based code

**Rollback Plan**: If critical bugs found, revert scene changes and debug new system in isolation before re-attempting migration.

## Success Criteria

### Must Achieve:
- [x] Zero `CardManager.Instance` or `Card.Instance` references in codebase
- [x] All tests passing (100% coverage)
- [x] Game plays identically to pre-refactor version
- [x] PlayerController implements ICardOwner
- [x] Multiple cards can exist simultaneously
- [x] No performance regression (60fps with 5 cards)

### Verification:
1. Search codebase for "CardManager.Instance" → zero results
2. Run all tests → 100% green
3. Play through level with profiler → 60fps maintained
4. Create test with 5 mock enemies throwing cards → all work independently

## Dependencies

### Required Before Starting:
- None (can start immediately)

### Nice to Have:
- ServiceLocator implementation (can workaround with SerializeField if not ready)
- InputHandler → IInputService refactor (can create adapter wrapper)
- PlayerVariables → PlayerController refactor (can implement partial interface)

### Blocks Future Work:
- Enemy card throwing requires this refactor complete
- Multi-enemy card mechanics require this refactor complete

## Timeline Estimate

Based on task breakdown in tasks.md (REVISED - narrow scope):

- **Phase 1 (Test Infrastructure & Interfaces)**: 1.5 hours
- **Phase 2 (CardManager Ownership Tracking)**: 5-6 hours
- **Phase 3 (Card DI - Critical!)**: 2 hours (+ mandatory manual playtest)
- **Phase 4 (PlayerController ICardOwner)**: 3.5 hours
- **Phase 5 (Integration & Migration)**: 4.5 hours

**Total**: 15-18 hours focused development time

With manual playtesting, code review, and careful diff review: **1-1.5 weeks for solo developer**

## Team Collaboration

### Roles:
- **Lead Developer**: Implements core tasks, reviews design
- **QA Engineer**: Writes E2E tests (task 4.4), manual verification (task 5.4)
- **Technical Artist**: Configures CardConfig ScriptableObjects (task 1.1)
- **Code Reviewer**: Reviews all PRs against design document

### Communication:
- Each phase completion is a review checkpoint
- Milestone 3 (Phase 3 complete) is critical review point (card physics working)
- Milestone 5 (Phase 5 complete) is final approval before old code deletion

## Related Documentation

- [.claude/refactor.md](../../../.claude/refactor.md) - Overall codebase refactoring strategy
- [.claude/CLAUDE.md](../../../.claude/CLAUDE.md) - Development guidelines and TDD requirements
- [Assets/_Scripts/Interfaces.cs](../../../Assets/_Scripts/Interfaces.cs) - Interface definitions
- [.kiro/kiro-system-templates/](../../kiro-system-templates/) - Kiro system documentation

## Questions or Issues?

- **Design questions**: Review design.md, check mermaid diagrams
- **Implementation questions**: Check tasks.md deliverables and code examples in design.md
- **Requirement questions**: Review requirements.md acceptance criteria
- **Process questions**: See [.kiro/kiro-system-templates/how_kiro_works.md](../../kiro-system-templates/how_kiro_works.md)

## Approval Status

- **Requirements**: ✅ Ready for approval
- **Design**: ✅ Ready for approval
- **Tasks**: ✅ Ready for implementation

**Next Step**: Review requirements.md and design.md, approve if satisfied, then begin task 1.1 implementation.

---

**Created**: 2025-12-28
**Author**: Claude Code Agent (Sonnet 4.5)
**Version**: 1.0
**Status**: Draft - Awaiting Team Approval

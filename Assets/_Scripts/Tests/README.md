# Card System Tests

This directory contains the test infrastructure for the Card System refactor.

## Structure

- **CardSystem.Tests.asmdef** - Test assembly definition that references Unity Test Framework
- **Mocks/** - Mock implementations for testing in isolation
  - **MockCardOwner.cs** - Mock implementation of ICardOwner interface
  - **MockCardEffectHandler.cs** - Mock implementation of CardEffectHandler
- **TestInfrastructureVerification.cs** - Tests to verify the test infrastructure works correctly

## Running Tests

1. Open Unity Editor
2. Go to Window > General > Test Runner
3. Select "EditMode" tab
4. Run all tests or individual test fixtures

## Test-Driven Development (TDD)

Per the project guidelines, all card system refactoring follows TDD:

1. **Write tests FIRST** - Define expected behavior with failing tests
2. **Implement code** - Make the tests pass
3. **Refactor** - Improve code while keeping tests green
4. **Never modify tests** - Tests define the contract; implementation adapts to tests

## Mock Classes

### MockCardOwner

Provides a test double for ICardOwner with tracking properties:
- `TeleportCalled` - Tracks if Teleport was called
- `ThrowCardCalled` - Tracks if ThrowCard was called
- `OnCardDestroyedCalled` - Tracks if OnCardDestroyed was called
- `CanThrowCard` - Configurable property for testing edge cases

Usage:
```csharp
var mockOwner = new MockCardOwner();
mockOwner.CanThrowCard = true;

// Test code here

mockOwner.Cleanup(); // Always cleanup!
```

### MockCardEffectHandler

Provides a test double for CardEffectHandler without requiring Unity prefabs:
- Tracks all effect method calls
- Records positions for assertion
- No actual particle instantiation

Usage:
```csharp
var mockHandler = new GameObject().AddComponent<MockCardEffectHandler>();

// Test code here

Object.DestroyImmediate(mockHandler.gameObject);
```

## Test Infrastructure Verification

The `TestInfrastructureVerification` test fixture ensures:
- Mock classes can be instantiated
- Tracking properties work correctly
- Reset methods clear all state
- No side effects between tests

## Next Steps

With test infrastructure in place, proceed to:
- Task 2.1: Write tests for CardManager ownership tracking
- Task 2.2: Implement CardManager ownership tracking
- Continue with TDD cycle through all remaining tasks

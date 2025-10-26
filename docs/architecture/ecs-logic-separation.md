# ECS Logic Separation Pattern

## Overview

When implementing game systems with Arch ECS, **always separate pure business logic from ECS orchestration**. This creates testable, maintainable, and reliable code.

## Architecture Pattern

### 1. Pure Logic Layer (No ECS Dependencies)

Create static classes in `apps/CookingProject.Logic/Logic/` with pure functions:

```csharp
namespace CookingProject.Logic.Logic;

/// <summary>
/// Pure business logic with no ECS dependencies.
/// All methods are pure functions that can be tested independently.
/// </summary>
public static class SharpeningLogic
{
    public static float CalculateSharpenAmount(float initialLevel, float maxLevel, float duration, float deltaTime)
    {
        float totalGap = maxLevel - initialLevel;
        float ratePerSecond = totalGap / duration;
        return ratePerSecond * deltaTime;
    }

    public static float ApplySharpeningProgress(float currentLevel, float sharpenAmount, float maxLevel)
    {
        float newLevel = currentLevel + sharpenAmount;
        return Math.Min(newLevel, maxLevel);
    }

    public static bool IsComplete(float elapsedTime, float duration)
    {
        return elapsedTime >= duration;
    }
}
```

**Key Characteristics:**
- ✅ Static methods (no state)
- ✅ Pure functions (same input = same output)
- ✅ No dependencies on `Arch.Core`, `World`, `Entity`, etc.
- ✅ Easy to unit test
- ✅ Fast to execute

### 2. ECS System Layer (Thin Wrapper)

Systems in `apps/CookingProject.Logic/Systems/` are thin orchestrators:

```csharp
using CookingProject.Logic.Logic; // Import pure logic

public class SharpeningSystem : IGameSystem
{
    private readonly World _world;
    private readonly GameFacade _facade;
    private readonly QueryDescription _query;

    public void Update(float deltaTime)
    {
        _world.Query(in _query, (ref Entity entity, ref Sharpness sharpness, ref SharpeningProgress progress) =>
        {
            // Call pure logic (testable without ECS)
            float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(
                progress.InitialLevel, sharpness.MaxLevel, progress.Duration, deltaTime);

            sharpness.Level = SharpeningLogic.ApplySharpeningProgress(
                sharpness.Level, sharpenAmount, sharpness.MaxLevel);

            progress.ElapsedTime += deltaTime;

            // Check completion using pure logic
            if (SharpeningLogic.IsComplete(progress.ElapsedTime, progress.Duration))
            {
                sharpness.Level = sharpness.MaxLevel;
                _facade.EmitEvent(new KnifeSharpenedEvent(entity.Id, sharpness.Level));
                entity.Remove<SharpeningProgress>();
            }
        });
    }
}
```

**System Responsibilities (ONLY):**
- ECS queries and component access
- Calling pure logic functions
- Updating components with results
- Emitting events
- Managing component lifecycle (Add/Remove)

**System Should NOT:**
- ❌ Contain business logic calculations
- ❌ Contain game rules or formulas
- ❌ Have complex conditionals (move to logic layer)
- ❌ Duplicate logic across multiple systems

## Testing Strategy

### Test Pure Logic (Fast, Comprehensive)

Create tests in `tests/CookingProject.Logic.Tests/Logic/`:

```csharp
public class SharpeningLogicTests
{
    [Fact]
    public void CalculateSharpenAmount_FromZeroToOne_Over5Seconds_Returns0Point2()
    {
        // Arrange
        float initialLevel = 0.0f;
        float maxLevel = 1.0f;
        float duration = 5.0f;
        float deltaTime = 1.0f;

        // Act
        float result = SharpeningLogic.CalculateSharpenAmount(initialLevel, maxLevel, duration, deltaTime);

        // Assert
        result.Should().BeApproximately(0.2f, 0.001f);
    }

    [Fact]
    public void Scenario_FullSharpening_From0To1_Over60Frames()
    {
        // Arrange - Simulate 60 fps for 5 seconds
        float initialLevel = 0.0f;
        float currentLevel = 0.0f;
        float maxLevel = 1.0f;
        float duration = 5.0f;
        float deltaTime = 1.0f / 60.0f;

        // Act - Simulate 300 frames
        for (int frame = 0; frame < 300; frame++)
        {
            float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(
                initialLevel, maxLevel, duration, deltaTime);
            currentLevel = SharpeningLogic.ApplySharpeningProgress(
                currentLevel, sharpenAmount, maxLevel);
        }

        // Assert
        currentLevel.Should().BeApproximately(1.0f, 0.01f);
    }
}
```

**Benefits:**
- No `World.Create()` or `World.Destroy()`
- No ECS threading issues
- No memory corruption or access violations
- Tests run in milliseconds
- Easy to test edge cases
- 100% coverage achievable

### Don't Test Systems Directly

**Why:**
- Systems are thin wrappers with minimal logic
- Testing requires complex ECS setup
- Prone to threading and lifecycle issues
- Slow and unreliable
- Low value compared to testing pure logic

**What to Test Instead:**
- ✅ Pure logic classes (comprehensive)
- ✅ Command handlers in GameFacade (integration tests with caution)
- ✅ Event emission (if needed, with minimal setup)

## Implementation Checklist

When implementing a new system:

### 1. Design Phase
- [ ] Define the game mechanic/feature in `docs/game-design/`
- [ ] Identify inputs, outputs, and formulas
- [ ] Sketch pure functions needed

### 2. Components Phase
- [ ] Create necessary components in `Components/`
- [ ] Keep components as simple data structures (public fields)
- [ ] Document each field's purpose

### 3. Pure Logic Phase
- [ ] Create `Logic/[Feature]Logic.cs` static class
- [ ] Implement pure functions for all calculations
- [ ] **Write comprehensive unit tests** (20+ tests minimum)
- [ ] Verify 100% coverage of logic layer

### 4. Commands & Events Phase
- [ ] Create command records in `Commands/`
- [ ] Create event records in `Events/`
- [ ] Document when each is used

### 5. System Phase
- [ ] Create thin `Systems/[Feature]System.cs`
- [ ] Query for relevant entities
- [ ] Call pure logic functions
- [ ] Update components and emit events
- [ ] **Keep this layer minimal!**

### 6. Integration Phase
- [ ] Register system in `GameFacade.Initialize()`
- [ ] Add command handlers in `GameFacade.ProcessCommand()`
- [ ] Test in Godot

## Example: Sharpening System

**Reference Implementation:**
- Pure Logic: `apps/CookingProject.Logic/Logic/SharpeningLogic.cs`
- ECS System: `apps/CookingProject.Logic/Systems/SharpeningSystem.cs`
- Components: `Components/Sharpness.cs`, `Components/SharpeningProgress.cs`
- Commands: `Commands/StartSharpeningCommand.cs`, `Commands/CancelSharpeningCommand.cs`
- Events: `Events/KnifeSharpenedEvent.cs`, etc.
- Tests: `tests/CookingProject.Logic.Tests/Logic/SharpeningLogicTests.cs` (22 tests, 100% coverage)

**Test Results:**
```
Passed: 22, Failed: 0, Duration: 164ms
```

## Why This Pattern Works

### Before (System with Logic Inside)
```csharp
public void Update(float deltaTime)
{
    _world.Query(..., (ref Entity entity, ref Sharpness sharpness, ref Progress progress) =>
    {
        // Logic mixed with ECS ❌
        float sharpenAmount = ((sharpness.MaxLevel - sharpness.Level) / progress.Duration) * deltaTime;
        sharpness.Level += sharpenAmount;
        sharpness.Level = Math.Min(sharpness.Level, sharpness.MaxLevel);

        if (progress.ElapsedTime >= progress.Duration)
        {
            // More logic...
        }
    });
}
```

**Problems:**
- Can't test logic without creating ECS World
- Tests crash due to threading issues
- Hard to test edge cases
- Slow test execution
- Logic duplicated if needed elsewhere

### After (Pure Logic + Thin System)
```csharp
// Pure logic (testable in 1ms)
public static class SharpeningLogic
{
    public static float CalculateSharpenAmount(...) { }
    public static float ApplySharpeningProgress(...) { }
    public static bool IsComplete(...) { }
}

// Thin system (orchestration only)
public void Update(float deltaTime)
{
    _world.Query(..., (ref Entity entity, ref Sharpness sharpness, ref Progress progress) =>
    {
        // Just call pure logic ✅
        float amount = SharpeningLogic.CalculateSharpenAmount(...);
        sharpness.Level = SharpeningLogic.ApplySharpeningProgress(...);

        if (SharpeningLogic.IsComplete(...))
        {
            // Handle completion...
        }
    });
}
```

**Benefits:**
- Logic tested independently (22 tests in 164ms)
- No ECS complications in tests
- Reusable logic
- Clear separation of concerns
- Easy to maintain and modify

## Common Patterns

### Calculation Functions
```csharp
public static float CalculateDamage(float baseDamage, float multiplier, float resistance)
{
    return baseDamage * multiplier * (1.0f - resistance);
}
```

### State Check Functions
```csharp
public static bool CanCraft(int requiredQuantity, int availableQuantity)
{
    return availableQuantity >= requiredQuantity;
}
```

### Progress Tracking Functions
```csharp
public static float CalculateProgress(float elapsedTime, float totalTime)
{
    return totalTime > 0 ? elapsedTime / totalTime : 1.0f;
}
```

### Clamping/Validation Functions
```csharp
public static float ClampToRange(float value, float min, float max)
{
    return Math.Clamp(value, min, max);
}
```

## Anti-Patterns to Avoid

### ❌ Complex Logic in Systems
```csharp
// BAD: Complex calculation in query callback
_world.Query(..., (ref Component c) =>
{
    float result = (c.Value * 2.5f + Math.Sqrt(c.OtherValue)) / (1.0f + c.ThirdValue);
    // Hard to test! ❌
});
```

### ❌ Testing Systems Directly
```csharp
// BAD: Testing system requires full ECS setup
public class SystemTests
{
    private World _world; // ❌ Prone to crashes
    private MySystem _system;

    [Fact]
    public void TestUpdate() // ❌ Unreliable
    {
        _world = World.Create(); // ❌ Slow
        // ...
    }
}
```

### ❌ Duplicating Logic
```csharp
// BAD: Same calculation in multiple systems
public class SystemA
{
    void Update() { float x = (a - b) / c; } // Duplicated ❌
}

public class SystemB
{
    void Update() { float x = (a - b) / c; } // Duplicated ❌
}
```

## Summary

**Golden Rule:** If it's a calculation, validation, or game rule → Pure Logic class
**System Rule:** If it's querying, updating, or orchestrating → ECS System

This pattern ensures:
- ✅ 90%+ test coverage is achievable
- ✅ Tests run in milliseconds
- ✅ No ECS-related test failures
- ✅ Code is reusable and maintainable
- ✅ Logic is decoupled from infrastructure

**Always follow this pattern for new systems!**

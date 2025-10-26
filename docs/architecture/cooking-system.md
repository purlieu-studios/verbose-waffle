# Cooking System Architecture

## Overview

The cooking system implements heat-based food preparation using the ECS Logic Separation pattern. It demonstrates complex state management with multiple components, time-based progression, and dynamic component addition/removal.

## File Structure

```
apps/CookingProject.Logic/Features/Cooking/
├── Components/
│   ├── HeatSource.cs           # Burner/stove component
│   ├── CookingProgress.cs      # Active cooking state
│   ├── BurnProgress.cs         # Burn damage tracking
│   ├── Container.cs            # Pan/pot component
│   └── CookingRequirements.cs  # Food-specific cooking data
├── Logic/
│   ├── HeatLogic.cs           # Pure heat validation/snapping
│   └── CookingLogic.cs        # Pure cooking calculations
├── Commands/
│   ├── SetHeatLevelCommand.cs
│   ├── PlaceFoodOnBurnerCommand.cs
│   └── RemoveFoodFromBurnerCommand.cs
├── Events/
│   ├── HeatLevelChangedEvent.cs
│   ├── FoodPlacedOnHeatEvent.cs
│   ├── FoodRemovedFromHeatEvent.cs
│   ├── CookingProgressEvent.cs
│   ├── BurningStartedEvent.cs
│   └── FoodCookedEvent.cs
└── CookingSystem.cs           # ECS orchestration

tests/CookingProject.Logic.Tests/Features/Cooking/Logic/
├── HeatLogicTests.cs          # 39 tests
└── CookingLogicTests.cs       # 40 tests
```

## Components

### HeatSource (Burner State)

```csharp
public struct HeatSource
{
    float CurrentHeat;           // 0.0, 0.33, 0.66, or 1.0
    Entity CookingEntity;        // What's on this burner
    bool HasCookingEntity;       // Burner occupied?
}
```

**Lifecycle:**
- Created when burner entity is initialized
- Persists for lifetime of burner
- Updated when heat changes or food placed/removed

### CookingProgress (Active Cooking)

```csharp
public struct CookingProgress
{
    float Doneness;              // 0.0 → 2.0
    float OptimalHeatMin;        // Copied from CookingRequirements
    float OptimalHeatMax;        // Copied from CookingRequirements
    float CookTimeSeconds;       // Copied from CookingRequirements
    bool IsOnHeat;               // Currently cooking?
    Entity StoveEntity;          // Which burner (if on heat)
}
```

**Lifecycle:**
- Added when food placed on burner (PlaceFoodOnBurnerCommand)
- Persists while cooking and cooling
- IsOnHeat toggled when removed from burner
- Could be removed when food is served or discarded (future)

### BurnProgress (Damage Tracking)

```csharp
public struct BurnProgress
{
    float BurnLevel;  // 0.0 → 1.0
}
```

**Lifecycle:**
- Dynamically added when doneness > 1.0
- Updated each frame while burning
- Removed if doneness drops below 1.0 (when cooling)

### CookingRequirements (Food Definition)

```csharp
public struct CookingRequirements
{
    bool RequiresContainer;
    ContainerType RequiredContainerType;  // Pan, Pot
    float OptimalHeatMin;
    float OptimalHeatMax;
    float CookTimeSeconds;
}
```

**Lifecycle:**
- Added to food entities at creation/initialization
- Immutable during gameplay
- Read-only reference data

## Pure Logic Layer

### HeatLogic.cs

**Purpose:** Heat level validation and management

**Functions:**
- `IsValidHeatLevel(float)` - Validates discrete heat values
- `SnapToValidHeatLevel(float)` - Snaps input to nearest valid level
- `IsHeatInOptimalRange(float, float, float)` - Checks optimal range

**Key Constants:**
```csharp
public const float HeatOff = 0.0f;
public const float HeatLow = 0.33f;
public const float HeatMedium = 0.66f;
public const float HeatHigh = 1.0f;
```

**Tests:** 39 tests covering validation, snapping, range checking, and scenarios

### CookingLogic.cs

**Purpose:** All cooking calculations and state checks

**Core Functions:**
- `CalculateCookingProgress(heat, cookTime, deltaTime)` - Doneness increase per frame
- `CalculateCoolingProgress(cookTime, deltaTime)` - Doneness decrease when off heat
- `ApplyDonenessChange(doneness, change)` - Clamps and updates doneness
- `IsPerfectlyCooked(doneness)` - Checks if doneness >= 1.0
- `IsBurning(doneness)` - Checks if doneness > 1.0
- `CalculateBurnLevel(doneness)` - Maps doneness to burn level
- `CalculateQuality(doneness)` - Quality score from doneness
- `CalculateProgressPercent(doneness)` - UI progress (0.0 → 1.0)

**Key Constants:**
```csharp
public const float PerfectDoneness = 1.0f;
public const float MaxDoneness = 2.0f;
public const float CoolingRateMultiplier = 0.3f;
```

**Formulas:**

*Cooking Speed:*
```csharp
cookingRate = currentHeat / cookTimeSeconds
doneness += cookingRate * deltaTime
```

*Cooling Speed (30% of base):*
```csharp
baseRate = 1.0 / cookTimeSeconds
coolingRate = baseRate * 0.3
doneness -= coolingRate * deltaTime
```

*Quality Calculation:*
```csharp
distanceFromPerfect = abs(doneness - 1.0)
quality = 1.0 - (distanceFromPerfect / max(1.0, 2.0 - 1.0))
quality = clamp(quality, 0.0, 1.0)
```

**Tests:** 40 tests covering cooking, cooling, burning, quality, and integration scenarios

## ECS System Layer

### CookingSystem.cs

**Responsibilities:**
- Query entities with CookingProgress
- For each food item:
  - If on heat: get heat source, calculate cooking, check burning
  - If off heat: calculate cooling
  - Emit progress events
  - Manage BurnProgress component lifecycle

**Update Flow:**
```
1. Query all entities with CookingProgress
2. For each entity:
   a. If IsOnHeat:
      - Get stove entity's HeatSource
      - Calculate cooking progress (pure logic)
      - Update doneness
      - Check if reached perfect doneness → emit FoodCookedEvent
      - Check if started burning → emit BurningStartedEvent, add BurnProgress
      - Update burn level if burning
      - Emit CookingProgressEvent
   b. If not on heat:
      - Calculate cooling progress (pure logic)
      - Update doneness (decrease)
      - Remove BurnProgress if doneness drops below 1.0
      - Emit CookingProgressEvent
```

**Key Code Patterns:**

*Dynamic Component Addition:*
```csharp
if (!CookingLogic.IsBurning(previousDoneness) &&
    CookingLogic.IsBurning(progress.Doneness))
{
    _facade.EmitEvent(new BurningStartedEvent(entity.Id));
    if (!_world.Has<BurnProgress>(entity))
    {
        _world.Add(entity, new BurnProgress { BurnLevel = 0.0f });
    }
}
```

*Event-driven State Transitions:*
```csharp
if (!CookingLogic.IsPerfectlyCooked(previousDoneness) &&
    CookingLogic.IsPerfectlyCooked(progress.Doneness))
{
    float quality = CookingLogic.CalculateQuality(progress.Doneness);
    _facade.EmitEvent(new FoodCookedEvent(entity.Id, progress.Doneness, quality));
}
```

## Commands & Handlers

### SetHeatLevelCommand

**Handler: GameFacade.HandleSetHeatLevel()**

Flow:
1. Validate burner entity exists and has HeatSource
2. Snap heat level to valid value using HeatLogic
3. Update HeatSource.CurrentHeat
4. Emit HeatLevelChangedEvent

### PlaceFoodOnBurnerCommand

**Handler: GameFacade.HandlePlaceFoodOnBurner()**

Flow:
1. Validate food and burner entities exist
2. Check food has CookingRequirements
3. Check burner has HeatSource and is not occupied
4. Check container requirements (TODO: full validation)
5. Add or update CookingProgress component:
   - Copy requirements (OptimalHeatMin/Max, CookTimeSeconds)
   - Set IsOnHeat = true
   - Link to burner entity
6. Link burner to food (HeatSource.CookingEntity, HasCookingEntity)
7. Emit FoodPlacedOnHeatEvent

### RemoveFoodFromBurnerCommand

**Handler: GameFacade.HandleRemoveFoodFromBurner()**

Flow:
1. Validate food entity exists and has CookingProgress
2. Check IsOnHeat (already removed = no-op)
3. Unlink from burner:
   - Get stove entity
   - Set HeatSource.HasCookingEntity = false
4. Update CookingProgress.IsOnHeat = false
5. Emit FoodRemovedFromHeatEvent

**Note:** CookingProgress persists to allow cooling

## Events

### HeatLevelChangedEvent
- **When:** Player adjusts burner heat
- **Purpose:** UI feedback, sound effects
- **Data:** burnerId, newHeatLevel

### FoodPlacedOnHeatEvent
- **When:** Food placed on burner
- **Purpose:** Animation, sizzle sound
- **Data:** foodId, burnerId

### FoodRemovedFromHeatEvent
- **When:** Food removed from burner
- **Purpose:** Stop sizzle sound, animation
- **Data:** foodId

### CookingProgressEvent
- **When:** Every frame while food has CookingProgress
- **Purpose:** UI progress bars, real-time feedback
- **Data:** foodId, doneness, isInOptimalRange

### BurningStartedEvent
- **When:** Doneness crosses 1.0 threshold
- **Purpose:** Warning sound, smoke particles
- **Data:** foodId

### FoodCookedEvent
- **When:** Doneness reaches 1.0 (perfect)
- **Purpose:** Success feedback, quality display
- **Data:** foodId, finalDoneness, quality

## Testing Strategy

### Pure Logic Tests (79 tests, 100% coverage)

**HeatLogicTests.cs (39 tests):**
- Heat level validation (valid/invalid values, tolerance)
- Heat level snapping (edge cases, midpoints)
- Optimal range checking (boundaries, narrow ranges)
- Integration scenarios (player actions, food requirements)

**CookingLogicTests.cs (40 tests):**
- Cooking progress calculation (different heat levels, edge cases)
- Cooling progress calculation
- Doneness change application (clamping, negative values)
- State checks (perfect, burning)
- Burn level calculation (gradual progression)
- Quality calculation (perfect, undercooked, overcooked)
- Integration scenarios (full cooking cycles, burning, cooling)

**Test Results:**
```
Passed: 79, Failed: 0, Duration: 116ms
```

### System Tests

**Not implemented** - Following ECS Logic Separation pattern, the thin system layer doesn't warrant testing. All game logic is tested via pure logic tests.

## Integration with GameFacade

**Registration:**
```csharp
public void Initialize()
{
    _systems.Add(new SharpeningSystem(_world, this));
    _systems.Add(new CookingSystem(_world, this)); // ← Added here
    // ...
}
```

**Command Routing:**
```csharp
public void ProcessCommand(IGameCommand command)
{
    switch (command)
    {
        case SetHeatLevelCommand cmd:
            HandleSetHeatLevel(cmd);
            break;
        case PlaceFoodOnBurnerCommand cmd:
            HandlePlaceFoodOnBurner(cmd);
            break;
        case RemoveFoodFromBurnerCommand cmd:
            HandleRemoveFoodFromBurner(cmd);
            break;
        // ...
    }
}
```

## Design Decisions

### 1. Discrete Heat Levels vs Continuous

**Decision:** Discrete levels (0.0, 0.33, 0.66, 1.0)

**Rationale:**
- Simpler player control (dial with 4 positions)
- Easier to balance (fewer heat combinations)
- Clear visual/audio feedback per level
- No heat drift = predictable behavior

**Alternatives Considered:**
- Continuous 0.0 → 1.0: Too complex for player input
- 5+ levels: Overwhelming, diminishing returns

### 2. Cooling Mechanics

**Decision:** Food cools at 30% of cooking speed when removed

**Rationale:**
- Prevents instant return to raw
- Allows strategic "pause and resume" cooking
- Realistic (food doesn't instantly cool)
- Creates interesting decisions (remove now or risk burning?)

**Alternatives Considered:**
- No cooling: Too forgiving, no consequence to leaving food off heat
- Faster cooling (50%+): Punishing, forces player to commit to heat
- Temperature-based cooling: Too complex for initial implementation

### 3. Burn Damage as Separate Component

**Decision:** BurnProgress component added dynamically when burning starts

**Rationale:**
- Memory efficient (only exists when needed)
- Clear lifecycle (added at 1.0, removed if doneness drops)
- Explicit state (entity.Has<BurnProgress> = is burning)
- Allows separate visual effects system to query burning entities

**Alternatives Considered:**
- BurnLevel in CookingProgress: Always allocates space, less clear state
- Separate Burning component (bool only): Redundant with BurnProgress.BurnLevel
- No separate tracking: Harder to query "all burning food" efficiently

### 4. Entity References vs IDs

**Decision:** Store Entity directly in components (HeatSource.CookingEntity)

**Rationale:**
- Arch.Core Entity is a lightweight struct
- Direct reference = no lookup needed
- Simpler code (no GetEntity() calls)
- HasCookingEntity bool makes null-checking explicit

**Alternatives Considered:**
- int? EntityId: Requires World.GetEntity() (doesn't exist in Arch)
- No linkage: Would require querying all food to find what's on burner

### 5. Container Validation

**Decision:** Check RequiresContainer but defer type validation

**Rationale:**
- Type validation needs container-food relationship
- UI layer can prevent invalid placements
- Keeps command handler simple
- Allows future enhancement without breaking changes

**TODO:** Full container type matching in PlaceFoodOnBurnerCommand

## Performance Characteristics

**Per Frame (60 fps):**
- CookingSystem queries all entities with CookingProgress
- For each entity: ~10 pure function calls + 1-2 component updates
- Expected load: 10-20 food items = ~200 function calls/frame
- Negligible overhead (pure functions are inlined)

**Memory:**
- HeatSource: 12 bytes (float + Entity + bool)
- CookingProgress: 24 bytes (5 floats + Entity + bool + padding)
- BurnProgress: 4 bytes (only when burning)
- Total per food item: 24-28 bytes (28 if burning)
- 100 food items: ~2.8 KB

**Event Emission:**
- CookingProgressEvent: Every frame per cooking item (UI updates)
- Other events: State transitions only (infrequent)
- Expected: 10-20 events/frame at peak

## Future Enhancements

### Planned Features
- **Container type validation:** Full pan/pot type checking
- **Multi-burner support:** Already supported (HeatSource per burner)
- **Oven cooking:** New component + system, similar pattern
- **Recipe integration:** CookingRequirements loaded from recipe data

### Possible Extensions
- **Heat transfer:** Burners take time to heat up/cool down
- **Pan temperature:** Separate from burner, affects cooking
- **Residual heat:** Food continues cooking briefly after removal
- **Stirring:** Player action to prevent burning at high heat
- **Lid usage:** Affects cooking speed/moisture

## Related Documentation

- Game Design: `docs/game-design/mechanics/heat-cooking.md`
- ECS Pattern: `docs/architecture/ecs-logic-separation.md`
- Sharpening System: Similar complexity, good reference

## Summary

The cooking system demonstrates:
- ✅ Pure logic separation (100% test coverage)
- ✅ Dynamic component management (BurnProgress)
- ✅ Event-driven state transitions
- ✅ Entity relationships (food ↔ burner)
- ✅ Time-based progression (cooking/cooling)
- ✅ Quality calculation (gameplay depth)

**Total Implementation:**
- 5 components
- 2 pure logic classes (300 LOC)
- 3 commands
- 6 events
- 1 ECS system (115 LOC)
- 79 tests (100% logic coverage)
- Build time: ~700ms
- Test time: 116ms

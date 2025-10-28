# ChoppingSystem Specification

## Purpose

Handles ingredient preparation through chopping, integrating with the knife sharpness system to create resource management gameplay.

## Components Required

**Must Have:**
- `Ingredient` - Defines ingredient properties (type, hardness, degradation)
- `ChoppableItem` - Tracks chop progress (required chops, current chops)
- `ChoppingProgress` - Active chopping state (knife, elapsed time, duration)
- `Sharpness` - Knife sharpness (used for time calculation and degradation)

**Optional:**
- `Position` - For spatial interaction (cutting board proximity)
- `Tool` - Identifies entity as a tool (knife filtering)

## System Behavior

### Initialization

```csharp
public ChoppingSystem(World world, GameFacade facade)
{
    _world = world;
    _facade = facade;

    // Query for entities actively being chopped
    _choppingQuery = new QueryDescription()
        .WithAll<Ingredient, ChoppableItem, ChoppingProgress>();
}
```

### Update Loop Logic

**For Each Entity Being Chopped:**

1. **Get Current State**
   ```csharp
   ref var ingredient = ref entity.Get<Ingredient>();
   ref var choppable = ref entity.Get<ChoppableItem>();
   ref var progress = ref entity.Get<ChoppingProgress>();
   ```

2. **Advance Progress**
   ```csharp
   progress.ElapsedTime += deltaTime;
   float progressPercent = progress.ElapsedTime / progress.ChopDuration;

   // Emit progress event for UI updates
   _facade.EmitEvent(new ChoppingProgressEvent(entity.Id, progressPercent));
   ```

3. **Check Completion**
   ```csharp
   if (progress.ElapsedTime >= progress.ChopDuration)
   {
       // Complete one chop
       choppable.CurrentChops++;

       // Degrade knife
       if (_world.Has<Sharpness>(progress.KnifeEntity))
       {
           ref var sharpness = ref _world.Get<Sharpness>(progress.KnifeEntity);
           sharpness.Level -= ingredient.DegradationAmount;
           sharpness.Level = Math.Max(0.0f, sharpness.Level);

           _facade.EmitEvent(new KnifeDegradedEvent(progress.KnifeEntity.Id, sharpness.Level));
       }

       // Check if fully chopped
       bool fullyChopped = choppable.CurrentChops >= choppable.RequiredChops;
       choppable.IsFullyChopped = fullyChopped;

       // Emit chop completion event
       _facade.EmitEvent(new IngredientChoppedEvent(
           entity.Id,
           choppable.CurrentChops,
           fullyChopped
       ));

       if (fullyChopped)
       {
           _facade.EmitEvent(new IngredientFullyPreparedEvent(entity.Id));
       }

       // Remove ChoppingProgress (chop complete)
       entity.Remove<ChoppingProgress>();
   }
   ```

### Edge Cases

**Cancelled Chopping:**
- Remove `ChoppingProgress` component without advancing chops
- Do NOT degrade knife (incomplete action)
- Emit `ChoppingCancelledEvent`

**Knife Removed Mid-Chop:**
- Check `_world.IsAlive(progress.KnifeEntity)` before degrading
- If knife missing, complete chop but skip degradation
- Log warning for debugging

**Already Fully Chopped:**
- StartChoppingCommand should check `choppable.IsFullyChopped`
- If true, reject command or complete immediately
- Prevent over-chopping

**Chops Exceed Required:**
- Clamp: `choppable.CurrentChops = Math.Min(currentChops, requiredChops)`
- Should never happen with proper completion check

## Commands Handled

### StartChoppingCommand
```csharp
public record StartChoppingCommand(Entity IngredientEntity, Entity KnifeEntity) : IGameCommand;
```

**Handling:**
1. Validate ingredient has Ingredient + ChoppableItem components
2. Validate knife has Sharpness component
3. Check ingredient not already being chopped (no ChoppingProgress)
4. Calculate chop duration based on knife sharpness:
   ```csharp
   float chopTime = ChoppingLogic.CalculateChopTime(
       ingredient.BaseChopTime,
       knife.Sharpness.Level
   );
   ```
5. Add ChoppingProgress component with calculated duration
6. Emit `ChoppingStartedEvent`

**Validation:**
- IngredientEntity must exist and be alive
- KnifeEntity must exist and be alive
- Ingredient must have Ingredient + ChoppableItem components
- Knife must have Sharpness component
- Ingredient must not already have ChoppingProgress (can't chop twice simultaneously)
- Ingredient must not already be fully chopped (optional - could allow re-chop)

### CancelChoppingCommand
```csharp
public record CancelChoppingCommand(Entity IngredientEntity) : IGameCommand;
```

**Handling:**
1. Check if entity has ChoppingProgress component
2. Get partial progress for event
3. Remove ChoppingProgress component
4. Emit `ChoppingCancelledEvent` with partial progress

**Validation:**
- IngredientEntity must exist
- Must have ChoppingProgress component (can only cancel active chops)

## Events Emitted

### ChoppingStartedEvent
```csharp
public record ChoppingStartedEvent(int IngredientId, int KnifeId, float Duration) : IGameEvent;
```
**When**: Player starts chopping
**Godot Response**: Show progress bar, play chopping animation/sound

### ChoppingProgressEvent
```csharp
public record ChoppingProgressEvent(int IngredientId, float Progress) : IGameEvent;
```
**When**: Every frame during chopping
**Godot Response**: Update progress bar fill amount

### IngredientChoppedEvent
```csharp
public record IngredientChoppedEvent(int IngredientId, int CurrentChops, bool FullyChopped) : IGameEvent;
```
**When**: Single chop completes
**Godot Response**: Play chop sound, update ingredient sprite, trigger particle effect

### IngredientFullyPreparedEvent
```csharp
public record IngredientFullyPreparedEvent(int IngredientId) : IGameEvent;
```
**When**: All required chops complete
**Godot Response**: Play completion sound, enable pickup/cooking, show visual indicator

### KnifeDegradedEvent
```csharp
public record KnifeDegradedEvent(int KnifeId, float NewSharpness) : IGameEvent;
```
**When**: Knife sharpness decreases from chopping
**Godot Response**: Update sharpness UI, change knife sprite if threshold crossed

### ChoppingCancelledEvent
```csharp
public record ChoppingCancelledEvent(int IngredientId, float PartialProgress) : IGameEvent;
```
**When**: Player cancels mid-chop
**Godot Response**: Hide progress bar, stop animation/sound

## Integration with Other Systems

### Sharpening System
- ChoppingSystem degrades Sharpness component
- SharpeningSystem restores Sharpness component
- No direct dependency (communicate via Sharpness component)

### Cooking System (Future)
- CookingSystem requires `choppable.IsFullyChopped == true`
- PlaceFoodOnBurnerCommand validates preparation state
- Unchopped ingredients cannot be cooked

### Movement System
- Player can move ingredients to cutting board (Position component)
- Proximity check before allowing StartChoppingCommand
- Cutting board has Area2D for interaction detection

## Performance Considerations

**Per Frame Overhead:**
- Query only entities with ChoppingProgress (~1-5 entities max)
- Each chopping entity: ~3 component lookups, 1-2 events
- Progress events = 60/second per chopping ingredient (acceptable)

**Optimization Opportunities:**
- Cache knife Sharpness lookup (don't query every frame)
- Batch event emissions if multiple chops complete same frame
- Use query filters to skip empty chops

**Memory:**
- ChoppingProgress: 16 bytes per active chop
- Events: Small allocations, garbage collected quickly
- No persistent allocations

## Testing Strategy

### Unit Tests (Logic Layer)

**ChoppingLogic.cs Pure Functions:**
- `CalculateChopTime(baseTime, sharpness)` - Various sharpness levels
- `CalculateProgress(elapsed, duration)` - Progress percentage
- `ShouldCompleteChop(elapsed, duration)` - Completion detection
- `CalculateDegradation(hardness)` - Degradation amount per hardness

**Test Scenarios:**
- Sharp knife (1.0): Base chop time
- Medium knife (0.5): ~54% slower
- Dull knife (0.0): ~233% slower
- Hard ingredient degradation (-0.08)
- Soft ingredient degradation (-0.03)
- Progress from 0% to 100% over multiple frames
- Partial chop then cancel (no degradation)

### Integration Tests (System Layer)

**ChoppingSystem.cs:**
- StartChoppingCommand → ChoppingStartedEvent
- Progress accumulation over 60 frames
- Chop completion → IngredientChoppedEvent + KnifeDegradedEvent
- Multiple chops (0 → 1 → 2 → 3 → 4 fully chopped)
- Cancel mid-chop → ChoppingCancelledEvent
- Already fully chopped ingredient (validation error)

### Godot Integration Tests

**test_chopping.tscn:**
- Click ingredient + cutting board starts chopping
- Progress bar fills from 0% to 100%
- Chop completes, sprite changes, sound plays
- Knife sharpness UI updates
- Multiple chops advance sprite through states
- Fully chopped ingredient shows different visual

## Future Enhancements

**Chopping Quality:**
- Perfect timing bonus (rhythm game mechanic)
- Critical chop: 2x speed, no degradation
- Failure penalty: wasted time, extra degradation

**Advanced Techniques:**
- Julienne: Thin strips (longer time, no extra degradation)
- Dice: Small cubes (standard)
- Rough chop: Fast, uneven (faster but lower quality)

**Ingredient Combos:**
- Batch chopping: Multiple ingredients at once
- Mise en place: Pre-chop during downtime, store in containers

**Upgrades:**
- Ceramic knife: Degrades 50% slower
- Professional knife: 10% faster base chop time
- Knife skills: Player upgrades reduce chop time

## Example Gameplay Flow

**Player receives order: "Vegetable Soup" (requires chopped carrot, onion, tomato)**

1. **Gather ingredients** - Place carrot, onion, tomato on cutting board
2. **Check knife** - Sharpness 0.7 (medium-sharp)
3. **Start chopping carrot:**
   - Click carrot + cutting board
   - GameController → StartChoppingCommand(carrot, knife)
   - ChoppingSystem → Calculate duration: 2.0s / (0.3 + 0.7*0.7) = 2.56s
   - ChoppingSystem → Add ChoppingProgress component
   - ChoppingSystem → Emit ChoppingStartedEvent
4. **Progress accumulates:**
   - Every frame: ElapsedTime += deltaTime
   - Every frame: Emit ChoppingProgressEvent (UI updates progress bar)
5. **First chop completes** (2.56 seconds elapsed):
   - CurrentChops: 0 → 1
   - Sharpness: 0.7 → 0.62 (-0.08 degradation)
   - Emit IngredientChoppedEvent (sprite changes to halved)
   - Emit KnifeDegradedEvent (UI shows new sharpness)
   - Remove ChoppingProgress
6. **Player clicks carrot again** - Repeat 3 more times
7. **Fourth chop completes:**
   - CurrentChops: 3 → 4
   - IsFullyChopped = true
   - Emit IngredientFullyPreparedEvent
   - Carrot sprite now shows diced cubes
8. **Repeat for onion and tomato** - Knife further degrades
9. **All ingredients prepared** - Player can now cook soup

# SharpeningSystem Specification

## Purpose

Handles the knife sharpening mechanic, including:
- Detecting when player initiates sharpening
- Tracking sharpening progress over time
- Restoring knife sharpness when complete
- Emitting events for UI/audio feedback

## Components Required

**Must Have:**
- `Sharpness` - Tracks current and max sharpness level
- `SharpeningProgress` - Tracks active sharpening state

**Optional:**
- `Tool` - Identifies entity as a tool (for filtering)

## System Behavior

### Initialization

```csharp
public SharpeningSystem(World world, GameFacade facade)
{
    _world = world;
    _facade = facade;

    // Query for entities being sharpened
    _sharpeningQuery = new QueryDescription()
        .WithAll<Sharpness, SharpeningProgress>();
}
```

### Update Loop Logic

**For Each Entity Being Sharpened:**

1. **Get Current State**
   ```csharp
   ref var sharpness = ref entity.Get<Sharpness>();
   ref var progress = ref entity.Get<SharpeningProgress>();
   ```

2. **Calculate Progress**
   ```csharp
   float sharpenAmount = ((sharpness.MaxLevel - sharpness.Level) / progress.Duration) * deltaTime;
   sharpness.Level += sharpenAmount;
   progress.ElapsedTime += deltaTime;
   ```

3. **Emit Progress Event** (every frame while sharpening)
   ```csharp
   float progressPercent = progress.ElapsedTime / progress.Duration;
   _facade.EmitEvent(new SharpeningProgressEvent(entity.Id, progressPercent));
   ```

4. **Check Completion**
   ```csharp
   if (progress.ElapsedTime >= progress.Duration)
   {
       sharpness.Level = sharpness.MaxLevel; // Restore to max
       _facade.EmitEvent(new KnifeSharpenedEvent(entity.Id, sharpness.Level));

       // Remove SharpeningProgress component (done sharpening)
       entity.Remove<SharpeningProgress>();
   }
   ```

### Edge Cases

**Interrupted Sharpening:**
- If player cancels: Remove `SharpeningProgress` component without changing sharpness
- Partial progress is lost (must start over)

**Already Sharp:**
- If sharpness is already at MaxLevel, sharpening does nothing (instant complete)
- Emit event immediately, remove progress component

**Sharpness Exceeds Max:**
- Clamp: `sharpness.Level = Math.Min(sharpness.Level, sharpness.MaxLevel)`

## Commands Handled

### StartSharpeningCommand
```csharp
public record StartSharpeningCommand(Entity KnifeEntity, float Duration) : IGameCommand;
```

**Handling:**
1. Check if knife entity exists and has Sharpness component
2. Add SharpeningProgress component if not already present
3. Emit `SharpeningStartedEvent`

**Validation:**
- Entity must exist
- Entity must have Sharpness component
- Entity should not already be sharpening (check for SharpeningProgress)

### CancelSharpeningCommand
```csharp
public record CancelSharpeningCommand(Entity KnifeEntity) : IGameCommand;
```

**Handling:**
1. Remove SharpeningProgress component
2. Emit `SharpeningCancelledEvent`

## Events Emitted

### SharpeningStartedEvent
```csharp
public record SharpeningStartedEvent(int EntityId, float Duration) : IGameEvent;
```
**When**: Player begins sharpening
**Godot Response**: Show sharpening UI/animation

### SharpeningProgressEvent
```csharp
public record SharpeningProgressEvent(int EntityId, float Progress) : IGameEvent;
```
**When**: Every frame during sharpening
**Godot Response**: Update progress bar, play scraping sound

### KnifeSharpenedEvent
```csharp
public record KnifeSharpenedEvent(int EntityId, float FinalSharpness) : IGameEvent;
```
**When**: Sharpening completes
**Godot Response**: Play completion sound, show gleam effect, hide sharpening UI

### SharpeningCancelledEvent
```csharp
public record SharpeningCancelledEvent(int EntityId, float PartialProgress) : IGameEvent;
```
**When**: Player cancels mid-sharpen
**Godot Response**: Hide sharpening UI, play cancel sound

## Integration with Other Systems

### ChoppingSystem Integration
- ChoppingSystem degrades Sharpness component
- SharpeningSystem restores Sharpness component
- No direct dependency between systems (communicate via components)

### UI Integration (Godot)
```csharp
case SharpeningProgressEvent e:
    _sharpeningProgressBar.Value = e.Progress;
    break;

case KnifeSharpenedEvent e:
    PlaySharpenCompleteSound();
    ShowKnifeGleamEffect(e.EntityId);
    break;
```

## Performance Considerations

- Sharpening is infrequent (every 30-60 seconds)
- Query only entities actively being sharpened (tiny subset)
- Progress events every frame = ~60/second (acceptable for 1-2 knives)

## Testing Strategy

**Unit Tests:**
- Sharpening from 0.0 to 1.0 takes correct duration
- Sharpening from 0.5 to 1.0 takes half duration
- Cancelling mid-sharpen doesn't change sharpness
- Cannot exceed MaxLevel

**Integration Tests:**
- StartSharpeningCommand → SharpeningStartedEvent
- Progress events emitted during sharpen
- KnifeSharpenedEvent on completion
- Sharpen + chop + sharpen works correctly

## Future Enhancements

- **Sharpening Quality Mini-game**: Perfect timing = bonus sharpness
- **Multiple Sharpening Stations**: Different stones, different speeds
- **Auto-sharpen Upgrade**: Knife sharpens passively when not in use

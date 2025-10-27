# Physics & Engine Boundary

## Architectural Constraint

**The logic layer (`CookingProject.Logic`) cannot reference Godot assemblies.**

This is a fundamental constraint because:
- It's a pure C# class library project
- Godot types are only available in Godot project assemblies
- **LibGodot (experimental feature to expose Godot as library) is not stable/supported**

## Layer Responsibilities

### Logic Layer (CookingProject.Logic)
**What it handles:**
- ✅ Game rules and state
- ✅ Position/velocity using custom `Vector2`
- ✅ ECS components, systems, commands, events
- ✅ Pure logic calculations (sharpening, cooking, etc.)
- ✅ Rule-based responses to collisions (damage, item pickup, etc.)

**What it does NOT handle:**
- ❌ Physics simulation
- ❌ Collision detection
- ❌ Raycasting
- ❌ Rendering
- ❌ Audio
- ❌ Input capture

### Presentation Layer (apps/game - Godot Project)
**What it handles:**
- ✅ Physics simulation (Godot's physics engine)
- ✅ Collision detection (Area2D, CollisionShape2D, PhysicsBody2D)
- ✅ Raycasting
- ✅ Rendering (sprites, tilemaps, shaders)
- ✅ Audio (sound effects, music)
- ✅ Input capture (keyboard, mouse, gamepad)
- ✅ Converting between `LogicVector2` ↔ `Godot.Vector2`

## Custom Math Library

We maintain our own math types in `CookingProject.Logic.Core.Math`:

**Current Types:**
- `Vector2` - 2D position/velocity/direction

**Future Types (as needed):**
- `Vector3` - 3D support if needed
- `Rect2` - Bounding boxes for logic-level checks
- `Transform2D` - Rotation and scale
- `Quaternion` - 3D rotations

**Why Custom Types:**
1. Logic layer can't reference Godot
2. Keeps logic fully unit testable
3. Engine-agnostic (theoretically portable)
4. No runtime dependency on Godot for tests

## Communication Pattern

### Commands (Godot → Logic)

Godot detects events and sends commands to logic:

```csharp
// Godot detects collision
void OnArea2DBodyEntered(Node2D body)
{
    var entityA = _nodeToEntityMap[this];
    var entityB = _nodeToEntityMap[body];

    // Send to logic layer
    _gameFacade.ProcessCommand(new CollisionOccurredCommand(entityA, entityB));
}

// Godot captures input
void _Input(InputEvent @event)
{
    if (@event is InputEventKey keyEvent && keyEvent.Pressed)
    {
        var direction = GetDirectionFromKey(keyEvent.Keycode);
        _gameFacade.ProcessCommand(new SetVelocityCommand(_playerEntity, direction * 100));
    }
}
```

### Events (Logic → Godot)

Logic emits events, Godot responds with visuals/audio:

```csharp
// Logic emits event
_gameFacade.EmitEvent(new EntityDamagedEvent(entityId, damage));

// Godot handles presentation
void HandleGameEvent(IGameEvent evt)
{
    if (evt is EntityDamagedEvent dmgEvent)
    {
        PlayDamageSound();
        FlashSprite(dmgEvent.EntityId);
        UpdateHealthBar(dmgEvent.EntityId);
    }
}
```

### State Sync (Logic → Godot)

Godot reads ECS state and updates visuals:

```csharp
void _Process(double delta)
{
    // Update logic
    _gameFacade.Update((float)delta);

    // Sync visuals
    var world = _gameFacade.World;
    foreach (var (entity, sprite) in _entityToSpriteMap)
    {
        if (world.Has<Position>(entity))
        {
            ref var position = ref world.Get<Position>(entity);
            // Convert LogicVector2 to Godot.Vector2
            sprite.Position = new Godot.Vector2(position.Value.X, position.Value.Y);
        }
    }
}
```

## Physics Implementation Strategy

### Movement
**Godot Handles:**
- Nothing (logic handles velocity-based movement)

**Logic Handles:**
- MovementSystem updates Position based on Velocity
- Commands set velocity based on input

**Why:** Simple velocity-based movement doesn't need physics simulation.

---

### Collision Detection
**Godot Handles:**
- Collision detection (Area2D, CollisionShape2D)
- Trigger enter/exit events
- Broadphase and narrowphase collision checks

**Logic Handles:**
- Game rules when collision occurs
- "What happens" not "did it collide"

**Example:**
```csharp
// Godot: Detects collision
void OnPlayerAreaEntered(Area2D area)
{
    if (area is Coin coin)
    {
        var coinEntity = _nodeToEntityMap[coin];
        _gameFacade.ProcessCommand(new PickupItemCommand(_playerEntity, coinEntity));
    }
}

// Logic: Handles rules
void HandlePickupItem(PickupItemCommand cmd)
{
    // Remove coin from world
    _world.Destroy(cmd.ItemEntity);

    // Add to inventory
    ref var inventory = ref _world.Get<Inventory>(cmd.PlayerEntity);
    inventory.Coins++;

    // Emit event for presentation
    EmitEvent(new CoinCollectedEvent(cmd.PlayerEntity));
}
```

---

### Raycasting
**Godot Handles:**
- Raycast execution (PhysicsRayQueryParameters2D)
- Hit detection and distance calculation

**Logic Handles:**
- Decision making based on raycast results
- "What to do with hit target"

**Example:**
```csharp
// Godot: Performs raycast
void CheckLineOfSight()
{
    var raycast = new PhysicsRayQueryParameters2D();
    raycast.From = _playerSprite.GlobalPosition;
    raycast.To = _enemySprite.GlobalPosition;

    var result = GetWorld2D().DirectSpaceState.IntersectRay(raycast);

    if (result.Count > 0)
    {
        var hitEntity = _nodeToEntityMap[result["collider"]];
        _gameFacade.ProcessCommand(new LineOfSightCommand(_playerEntity, hitEntity, true));
    }
}
```

---

### Pathfinding
**Godot Handles:**
- A* navigation (NavigationAgent2D)
- Grid/navmesh traversal
- Path calculation

**Logic Handles:**
- Decision to pathfind
- Target selection
- Behavior when path complete/blocked

**Example:**
```csharp
// Godot: Calculate path
void MoveToTarget(Vector2 target)
{
    _navAgent.TargetPosition = target;
}

void _on_navigation_agent_2d_velocity_computed(Vector2 safeVelocity)
{
    // Convert to LogicVector2 and send to logic
    var logicVelocity = new LogicVector2(safeVelocity.X, safeVelocity.Y);
    _gameFacade.ProcessCommand(new SetVelocityCommand(_entity, logicVelocity));
}
```

---

### Physics Bodies (CharacterBody2D, RigidBody2D)
**Godot Handles:**
- Gravity simulation
- Friction/drag
- Bounce/restitution
- Continuous collision detection
- `move_and_slide()` / `move_and_collide()`

**Logic Handles:**
- Game rules when physics events occur
- Velocity targets/goals

**Decision:**
- ⚠️ **For now, we use simple velocity-based movement (no physics bodies)**
- ⚠️ **If we need physics bodies later, Godot handles simulation, logic sets goals**

**Future Example:**
```csharp
// Godot: Apply physics
void _PhysicsProcess(double delta)
{
    // Get desired velocity from logic
    var world = _gameFacade.World;
    ref var velocity = ref world.Get<Velocity>(_entity);

    // Let Godot physics handle collision response
    _characterBody.Velocity = new Godot.Vector2(velocity.Value.X, velocity.Value.Y);
    _characterBody.MoveAndSlide();

    // If Godot changed velocity (collision), update logic
    if (_characterBody.Velocity != savedVelocity)
    {
        var newLogicVel = new LogicVector2(_characterBody.Velocity.X, _characterBody.Velocity.Y);
        _gameFacade.ProcessCommand(new SetVelocityCommand(_entity, newLogicVel));
    }
}
```

---

### Spatial Queries (Area Searches, Overlaps)
**Godot Handles:**
- Spatial queries (who's near me?)
- Area overlaps
- Shape casting

**Logic Handles:**
- "What to do with nearby entities"
- AI decision making

**Example:**
```csharp
// Godot: Find nearby enemies
void UpdateAwareness()
{
    var nearbyBodies = _detectionArea.GetOverlappingBodies();
    var enemyEntities = new List<Entity>();

    foreach (var body in nearbyBodies)
    {
        if (body.IsInGroup("enemies"))
        {
            enemyEntities.Add(_nodeToEntityMap[body]);
        }
    }

    _gameFacade.ProcessCommand(new UpdateAwarenessCommand(_entity, enemyEntities));
}
```

---

## Testing Strategy

### Logic Layer Tests
- ✅ Pure unit tests with xUnit
- ✅ No Godot runtime required
- ✅ Fast (152 tests in <2 seconds)
- ✅ Can run in CI/CD

**Example:**
```csharp
[Fact]
public void MovementSystem_UpdatesPosition()
{
    var entity = _world.Create(
        new Position(0, 0),
        new Velocity(10, 0)
    );

    _movementSystem.Update(1.0f);

    ref var position = ref _world.Get<Position>(entity);
    position.Value.X.Should().Be(10f);
}
```

### Integration Tests (Godot Project)
- Manual testing in running game
- Scene-based test scenarios
- Visual validation
- Future: Automated scene tests via GDAI MCP

---

## Migration Path (If LibGodot Stabilizes)

**IF** Godot ever supports LibGodot as a stable library:

1. Add Godot as NuGet package to logic layer
2. Replace `CookingProject.Logic.Core.Math.Vector2` with `Godot.Vector2`
3. Find/replace `LogicVector2` → `Godot.Vector2` in game project
4. Remove conversion code
5. Tests can reference Godot types

**Status as of 2024/2025:**
- ⚠️ LibGodot is experimental (Godot 4.6-dev)
- ⚠️ Not officially supported
- ⚠️ No stable release timeline
- ⚠️ **Do not block development on this**

---

## Decision Log

### 2025-10-26: Custom Vector2
**Decision:** Use custom `CookingProject.Logic.Core.Math.Vector2`

**Rationale:**
- Logic layer cannot reference Godot assemblies
- LibGodot is experimental and unsupported
- Keeps logic fully unit testable
- Clean separation of concerns

**Trade-offs:**
- ❌ Conversion overhead (minimal - just X/Y copy)
- ❌ Can't use Godot vector helpers in logic
- ✅ 100% unit testable
- ✅ No Godot dependency for tests
- ✅ Engine-agnostic (theoretically)

**Revisit when:**
- LibGodot reaches stable release
- Conversion overhead becomes measurable bottleneck
- Need Godot-specific math in logic layer

### Future Physics Decisions
**To be decided:**
- Do we use CharacterBody2D or pure velocity?
- Do we implement jump physics?
- Do we need rigid body simulation?
- Do we implement swimming/flying physics?

**Guiding principle:**
- Godot simulates physics
- Logic decides goals and rules
- Commands bridge the gap

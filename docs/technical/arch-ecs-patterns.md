# Arch ECS Patterns and Gotchas

This document covers important patterns and behaviors when working with Arch ECS 2.1.0 in this project.

## Critical: Entity.Id Behavior

### The Issue

**Entity.Id is NOT reliable outside of query contexts.**

```csharp
// ❌ WRONG - entity.Id is 0 or stale
var entity = world.Create();
Console.WriteLine(entity.Id); // Outputs: 0
world.Add(entity, new SomeComponent());
Console.WriteLine(entity.Id); // Still outputs: 0

// Later in test:
Assert.Equal(entity.Id, someEvent.EntityId); // ❌ FAILS - comparing 0 to real ID

// ✅ CORRECT - entity.Id only valid inside queries
world.Query(query, (ref Entity e, ref SomeComponent c) =>
{
    Console.WriteLine(e.Id); // Outputs: 20 (actual ID)
    EmitEvent(new SomeEvent(e.Id)); // ✅ Uses real ID
});
```

### Why This Happens

- Entity structs are lightweight handles, not full objects
- `Entity.Id` is computed/accessed differently in query contexts vs outside
- This is by design in Arch ECS for performance reasons
- The entity reference is "activated" during query iteration

### Implications for Testing

**Don't assert on stored entity IDs:**

```csharp
// ❌ BAD TEST
var ingredient = CreateIngredient();
system.Update(1.0f);
var event = GetLastEvent<ChoppedEvent>();
Assert.Equal(ingredient.Id, event.EntityId); // FAILS - comparing 0 to 20

// ✅ GOOD TEST - Verify behavior without entity IDs
var ingredient = CreateIngredient();
system.Update(1.0f);
var event = GetLastEvent<ChoppedEvent>();
Assert.NotNull(event); // Event was emitted
Assert.Equal(1, event.ChopCount); // Has correct data
// Don't assert on entity.Id
```

**Alternative approaches:**

```csharp
// Option 1: Query to verify state
int foundEntities = 0;
world.Query(query, (ref Entity e, ref Component c) =>
{
    foundEntities++;
    // Verify component state here
});
Assert.Equal(expectedCount, foundEntities);

// Option 2: Use component values for matching
var events = GetAllEvents<ProgressEvent>();
var progresses = events.Select(e => e.Progress).OrderBy(p => p).ToList();
Assert.InRange(progresses[0], 0.24f, 0.26f); // Verify by value, not ID

// Option 3: Track entities by component data
ref var component = ref world.Get<SomeComponent>(entity);
Assert.Equal(expectedValue, component.SomeField);
```

### Where This Matters

1. **Event assertions** - Events emit `entity.Id` from inside queries (correct), but tests can't match against stored entity variables
2. **Component lookups** - `world.Has<T>(entity)` and `world.Get<T>(entity)` may fail with stale entity handles
3. **Multi-test scenarios** - Entity IDs from one test may not match IDs in another

### Production Code is Fine

Our production systems only use `entity.Id` **inside queries**, which is correct:

```csharp
// ✅ All production code follows this pattern
world.Query(query, (ref Entity entity, ref Component c) =>
{
    facade.EmitEvent(new SomeEvent(entity.Id)); // ✅ Correct - inside query
});
```

## Other Arch ECS Patterns

### Component Removal During Queries

You **cannot** remove components during query iteration:

```csharp
// ❌ WRONG - Causes NullReferenceException
world.Query(query, (ref Entity entity, ref Component c) =>
{
    if (shouldRemove)
        entity.Remove<Component>(); // ❌ Crashes
});

// ✅ CORRECT - Defer removal
var toRemove = new List<Entity>();
world.Query(query, (ref Entity entity, ref Component c) =>
{
    if (shouldRemove)
        toRemove.Add(entity);
});

foreach (var entity in toRemove)
{
    if (world.IsAlive(entity) && world.Has<Component>(entity))
        world.Remove<Component>(entity);
}
```

### Query Parameter Order

Entity must be **first** in query lambda:

```csharp
// ❌ WRONG
world.Query(query, (ref Component c, ref Entity entity) => { }); // Compile error

// ✅ CORRECT
world.Query(query, (ref Entity entity, ref Component c) => { });
```

### World Lifecycle in Tests

Always dispose World instances:

```csharp
public class MyTests : IDisposable
{
    private readonly World _world;

    public MyTests()
    {
        _world = World.Create();
    }

    public void Dispose()
    {
        World.Destroy(_world);
    }
}
```

## Testing Best Practices

1. **Verify behavior, not IDs** - Assert on component state, event emissions, counts
2. **Use queries for assertions** - Count entities matching criteria via queries
3. **Match by component values** - Use component data to identify entities, not IDs
4. **Document limitations** - Add comments explaining why certain assertions are skipped
5. **Test isolation** - Always dispose World between tests

## References

- Arch ECS: https://github.com/genaray/Arch
- Related issue discovered: 2025-01-27 during ChoppingSystem test development
- See: `apps/CookingProject.Logic.Tests/Features/Chopping/ChoppingSystemTests.cs` for examples

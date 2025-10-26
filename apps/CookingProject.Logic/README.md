# CookingProject.Logic - Arch ECS Game Backend

This class library contains all the game logic using Arch ECS, separated from the Godot presentation layer.

## Architecture Overview

```
Godot (Presentation) ←→ GameFacade ←→ Arch ECS (Game Logic)
     UI/Rendering    Commands/Events    World/Systems/Components
```

### Command-Event Bridge Pattern

**Commands (Godot → ECS)** - Player intents and input
- `ChopIngredientCommand` - Chop an ingredient
- `StartCookingCommand` - Begin cooking a recipe
- `AdjustHeatCommand` - Change stove temperature

**Events (ECS → Godot)** - State changes to display
- `IngredientChoppedEvent` - Ingredient was chopped
- `CookingProgressEvent` - Cooking progress updated
- `IngredientBurnedEvent` - Ingredient burned
- `RecipeCompletedEvent` - Recipe finished successfully

## Project Structure

```
CookingProject.Logic/
├── Commands/           # Commands from Godot to ECS
│   ├── IGameCommand.cs
│   ├── ChopIngredientCommand.cs
│   ├── StartCookingCommand.cs
│   └── AdjustHeatCommand.cs
├── Events/             # Events from ECS to Godot
│   ├── IGameEvent.cs
│   ├── IngredientChoppedEvent.cs
│   ├── CookingProgressEvent.cs
│   ├── IngredientBurnedEvent.cs
│   └── RecipeCompletedEvent.cs
├── Components/         # ECS components (data)
│   ├── Ingredient.cs
│   ├── Temperature.cs
│   ├── CookingProgress.cs
│   ├── Recipe.cs
│   └── Burned.cs
├── Systems/            # ECS systems (logic)
│   ├── IGameSystem.cs
│   └── CookingSystem.cs
└── GameFacade.cs       # Bridge between Godot and ECS
```

## Usage from Godot

### 1. Initialize in _Ready()

```csharp
private GameFacade _gameFacade;

public override void _Ready()
{
    _gameFacade = new GameFacade();
    _gameFacade.Initialize();

    // Create initial game entities
    int tomatoId = _gameFacade.CreateTestIngredient("Tomato");
}
```

### 2. Update in _Process()

```csharp
public override void _Process(double delta)
{
    // Update ECS systems
    _gameFacade.Update((float)delta);

    // Get events and update UI
    var events = _gameFacade.ConsumeEvents();
    foreach (var evt in events)
    {
        HandleGameEvent(evt);
    }
}
```

### 3. Send Commands

```csharp
// When player clicks ingredient
_gameFacade.ProcessCommand(new ChopIngredientCommand(ingredientId));

// When player starts cooking
_gameFacade.ProcessCommand(new StartCookingCommand(recipeId, 0.8f));

// When player adjusts heat
_gameFacade.ProcessCommand(new AdjustHeatCommand(stoveId, 0.5f));
```

### 4. Handle Events

```csharp
private void HandleGameEvent(IGameEvent evt)
{
    switch (evt)
    {
        case IngredientChoppedEvent e:
            PlayChopAnimation(e.EntityId);
            PlayChopSound();
            break;

        case CookingProgressEvent e:
            UpdateProgressBar(e.Progress);
            break;

        case IngredientBurnedEvent e:
            ShowBurnedUI(e.IngredientName);
            PlayBurnSound();
            break;

        case RecipeCompletedEvent e:
            ShowSuccessUI(e.RecipeName, e.Score);
            break;
    }
}
```

## ECS Concepts

### Components (Data)
Structs that contain only data, no logic. Attached to entities.

```csharp
public struct Temperature
{
    public float Current;
    public float Target;
    public float HeatLevel;
}
```

### Systems (Logic)
Classes that operate on entities with specific components.

```csharp
public class CookingSystem : IGameSystem
{
    public void Update(float deltaTime)
    {
        // Query entities with specific components
        _world.Query(in _query, (ref Temperature temp, ref CookingProgress progress) =>
        {
            // Update cooking logic here
        });
    }
}
```

### Queries
Filter entities by component requirements.

```csharp
var query = new QueryDescription()
    .WithAll<Ingredient, Temperature>()  // Must have these
    .WithNone<Burned>();                 // Must not have this
```

## Adding New Features

### Add a New Command

1. Create in `Commands/MyCommand.cs`:
   ```csharp
   public record MyCommand(int EntityId, string Data) : IGameCommand;
   ```

2. Handle in `GameFacade.ProcessCommand()`:
   ```csharp
   case MyCommand cmd:
       HandleMyCommand(cmd);
       break;
   ```

### Add a New Event

1. Create in `Events/MyEvent.cs`:
   ```csharp
   public record MyEvent(int EntityId, string Data) : IGameEvent;
   ```

2. Emit from system:
   ```csharp
   _facade.EmitEvent(new MyEvent(entity.Id, "data"));
   ```

3. Handle in Godot's `HandleGameEvent()`.

### Add a New System

1. Create in `Systems/MySystem.cs`:
   ```csharp
   public class MySystem : IGameSystem
   {
       public void Update(float deltaTime) { /* ... */ }
   }
   ```

2. Register in `GameFacade.Initialize()`:
   ```csharp
   _systems.Add(new MySystem(_world, this));
   ```

## Benefits

✅ **Testable** - Systems can be unit tested without Godot
✅ **Performant** - Arch ECS is highly optimized
✅ **Maintainable** - Clear separation between logic and presentation
✅ **Flexible** - Easy to add/modify features
✅ **Portable** - Could swap Godot for Unity/MonoGame later

## Resources

- [Arch ECS Documentation](https://arch-ecs.gitbook.io/arch)
- [Arch GitHub Repository](https://github.com/genaray/Arch)
- Arch source code: `external/arch/` in this repository

using Arch.Core;
using CookingProject.Logic.Commands;
using CookingProject.Logic.Components;
using CookingProject.Logic.Events;
using CookingProject.Logic.Systems;

namespace CookingProject.Logic;

/// <summary>
/// Facade class that bridges Godot (presentation layer) and Arch ECS (game logic).
/// Godot sends commands via ProcessCommand(), receives events via ConsumeEvents(),
/// and drives the simulation via Update().
/// </summary>
public class GameFacade
{
    private readonly World _world;
    private readonly List<IGameSystem> _systems;
    private readonly Queue<IGameEvent> _eventQueue;
    private bool _isInitialized;

    /// <summary>
    /// Creates a new GameFacade instance.
    /// Call Initialize() after construction to set up the ECS world and systems.
    /// </summary>
    public GameFacade()
    {
        _world = World.Create();
        _systems = new List<IGameSystem>();
        _eventQueue = new Queue<IGameEvent>();
    }

    /// <summary>
    /// Initializes the ECS world and registers all game systems.
    /// Must be called before ProcessCommand() or Update().
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("GameFacade has already been initialized");
        }

        // Register systems in the order they should be updated
        _systems.Add(new CookingSystem(_world, this));
        _systems.Add(new SharpeningSystem(_world, this));

        // Future systems can be added here:
        // _systems.Add(new RecipeSystem(_world, this));
        // _systems.Add(new InventorySystem(_world, this));

        _isInitialized = true;
    }

    /// <summary>
    /// Updates all systems. Should be called every frame from Godot's _Process().
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame in seconds.</param>
    public void Update(float deltaTime)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("GameFacade must be initialized before Update()");
        }

        foreach (var system in _systems)
        {
            system.Update(deltaTime);
        }
    }

    /// <summary>
    /// Processes a command from Godot (player input/intent).
    /// Commands are routed to the appropriate systems or entities.
    /// </summary>
    /// <param name="command">The command to process.</param>
    public void ProcessCommand(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_isInitialized)
        {
            throw new InvalidOperationException("GameFacade must be initialized before ProcessCommand()");
        }

        switch (command)
        {
            case ChopIngredientCommand chopCmd:
                HandleChopIngredient(chopCmd);
                break;

            case StartCookingCommand cookCmd:
                HandleStartCooking(cookCmd);
                break;

            case AdjustHeatCommand heatCmd:
                HandleAdjustHeat(heatCmd);
                break;

            case StartSharpeningCommand sharpenCmd:
                HandleStartSharpening(sharpenCmd);
                break;

            case CancelSharpeningCommand cancelCmd:
                HandleCancelSharpening(cancelCmd);
                break;

            default:
                throw new NotSupportedException($"Command type {command.GetType().Name} is not supported");
        }
    }

    /// <summary>
    /// Consumes all pending events. Godot should call this each frame after Update()
    /// to retrieve state changes and update the UI accordingly.
    /// </summary>
    /// <returns>List of events that occurred this frame. Queue is cleared after calling.</returns>
    public List<IGameEvent> ConsumeEvents()
    {
        var events = _eventQueue.ToList();
        _eventQueue.Clear();
        return events;
    }

    /// <summary>
    /// Emits an event to be consumed by Godot.
    /// Called by systems when game state changes that the UI should react to.
    /// </summary>
    /// <param name="gameEvent">The event to emit.</param>
    internal void EmitEvent(IGameEvent gameEvent)
    {
        _eventQueue.Enqueue(gameEvent);
    }

    /// <summary>
    /// Creates a test ingredient entity for demonstration purposes.
    /// In a real game, this would be called from a system or initialization code.
    /// </summary>
    /// <param name="name">The ingredient name.</param>
    /// <returns>The created entity.</returns>
    public Entity CreateTestIngredient(string name)
    {
        var entity = _world.Create(
            new Ingredient { Name = name, IsChopped = false },
            new Temperature { Current = 20f, Target = 100f, HeatLevel = 0f },
            new CookingProgress { Progress = 0f, TimeCooked = 0f, RequiredTime = 30f, IsCooking = false }
        );

        return entity;
    }

    private void HandleChopIngredient(ChopIngredientCommand command)
    {
        var entity = command.IngredientEntity;

        if (!_world.IsAlive(entity))
        {
            return; // Entity doesn't exist
        }

        if (_world.Has<Ingredient>(entity))
        {
            ref var ingredient = ref _world.Get<Ingredient>(entity);
            ingredient.IsChopped = true;

            EmitEvent(new IngredientChoppedEvent(entity.Id, ingredient.Name));
        }
    }

    private void HandleStartCooking(StartCookingCommand command)
    {
        var entity = command.RecipeEntity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        if (_world.Has<CookingProgress>(entity) && _world.Has<Temperature>(entity))
        {
            ref var progress = ref _world.Get<CookingProgress>(entity);
            ref var temp = ref _world.Get<Temperature>(entity);

            progress.IsCooking = true;
            temp.HeatLevel = command.HeatLevel;
        }
    }

    private void HandleAdjustHeat(AdjustHeatCommand command)
    {
        var entity = command.Entity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        if (_world.Has<Temperature>(entity))
        {
            ref var temp = ref _world.Get<Temperature>(entity);
            temp.HeatLevel = Math.Clamp(command.HeatLevel, 0f, 1f);
        }
    }

    private void HandleStartSharpening(StartSharpeningCommand command)
    {
        var entity = command.KnifeEntity;

        if (!_world.IsAlive(entity))
        {
            return; // Entity doesn't exist
        }

        if (!_world.Has<Sharpness>(entity))
        {
            return; // Entity doesn't have Sharpness component
        }

        // Don't start if already sharpening
        if (_world.Has<SharpeningProgress>(entity))
        {
            return;
        }

        // Capture current sharpness level before starting
        ref var sharpness = ref _world.Get<Sharpness>(entity);

        // Add SharpeningProgress component to begin sharpening
        _world.Add(entity, new SharpeningProgress
        {
            InitialLevel = sharpness.Level,
            ElapsedTime = 0f,
            Duration = command.Duration
        });

        EmitEvent(new SharpeningStartedEvent(entity.Id, command.Duration));
    }

    private void HandleCancelSharpening(CancelSharpeningCommand command)
    {
        var entity = command.KnifeEntity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        if (_world.Has<SharpeningProgress>(entity))
        {
            ref var progress = ref _world.Get<SharpeningProgress>(entity);
            float partialProgress = progress.ElapsedTime / progress.Duration;

            // Remove SharpeningProgress component (cancel sharpening)
            _world.Remove<SharpeningProgress>(entity);

            EmitEvent(new SharpeningCancelledEvent(entity.Id, partialProgress));
        }
    }

    /// <summary>
    /// Disposes the ECS world and cleans up resources.
    /// Should be called when the game ends.
    /// </summary>
    public void Dispose()
    {
        World.Destroy(_world);
    }
}

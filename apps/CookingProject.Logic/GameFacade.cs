using Arch.Core;
using CookingProject.Logic.Core.Commands;
using CookingProject.Logic.Core.Components;
using CookingProject.Logic.Core.Events;
using CookingProject.Logic.Core.Systems;
using CookingProject.Logic.Features.Cooking;
using CookingProject.Logic.Features.Cooking.Commands;
using CookingProject.Logic.Features.Cooking.Components;
using CookingProject.Logic.Features.Cooking.Events;
using CookingProject.Logic.Features.Movement;
using CookingProject.Logic.Features.Sharpening;
using CookingProject.Logic.Features.Sharpening.Commands;
using CookingProject.Logic.Features.Sharpening.Components;
using CookingProject.Logic.Features.Sharpening.Events;

namespace CookingProject.Logic;

/// <summary>
/// Facade class that bridges Godot (presentation layer) and Arch ECS (game logic).
/// Godot sends commands via ProcessCommand(), receives events via ConsumeEvents(),
/// and drives the simulation via Update().
/// </summary>
public partial class GameFacade
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
        _systems.Add(new MovementSystem(_world));
        _systems.Add(new SharpeningSystem(_world, this));
        _systems.Add(new CookingSystem(_world, this));

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
    /// Provides read-only access to the ECS world for querying component data.
    /// Used by GameController to sync visual representations with ECS state.
    /// </summary>
    public World World => _world;

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
    /// Disposes the ECS world and cleans up resources.
    /// Should be called when the game ends.
    /// </summary>
    public void Dispose()
    {
        World.Destroy(_world);
    }
}

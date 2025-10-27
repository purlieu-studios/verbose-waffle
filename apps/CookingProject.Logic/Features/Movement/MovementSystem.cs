using System.Diagnostics.CodeAnalysis;
using Arch.Core;
using CookingProject.Logic.Core.Systems;
using CookingProject.Logic.Features.Movement.Components;

namespace CookingProject.Logic.Features.Movement;

/// <summary>
/// System that updates entity positions based on their velocity.
/// Runs every frame and applies velocity * deltaTime to position.
/// </summary>
public class MovementSystem : IGameSystem
{
    private readonly World _world;
    private readonly QueryDescription _movementQuery;

    public MovementSystem(World world)
    {
        _world = world;
        _movementQuery = new QueryDescription().WithAll<Position, Velocity>();
    }

    public void Update(float deltaTime)
    {
        // Update all entities with both Position and Velocity components
        _world.Query(in _movementQuery, (ref Position position, ref Velocity velocity) =>
        {
            // Apply velocity: position += velocity * deltaTime
            position.Value += velocity.Value * deltaTime;
        });
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Required by IDisposable pattern")]
    public void Dispose()
    {
        // No unmanaged resources to dispose
    }
}

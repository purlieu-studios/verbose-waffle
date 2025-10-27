using Arch.Core;
using CookingProject.Logic.Core.Math;
using CookingProject.Logic.Features.Movement.Commands;
using CookingProject.Logic.Features.Movement.Components;

namespace CookingProject.Logic;

/// <summary>
/// Movement-related command handlers for GameFacade.
/// </summary>
public partial class GameFacade
{
    private void HandleSetVelocity(SetVelocityCommand command)
    {
        var entity = command.Entity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        // Add or update Velocity component
        if (_world.Has<Velocity>(entity))
        {
            ref var velocity = ref _world.Get<Velocity>(entity);
            velocity.Value = command.Velocity;
        }
        else
        {
            _world.Add(entity, new Velocity(command.Velocity));
        }
    }

    private void HandleSetPosition(SetPositionCommand command)
    {
        var entity = command.Entity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        // Add or update Position component
        if (_world.Has<Position>(entity))
        {
            ref var position = ref _world.Get<Position>(entity);
            position.Value = command.Position;
        }
        else
        {
            _world.Add(entity, new Position(command.Position));
        }
    }

    /// <summary>
    /// Helper method to create a test moving entity.
    /// </summary>
    public Entity CreateTestMovingEntity(string name, Vector2 startPosition)
    {
        var entity = _world.Create(
            new Position(startPosition),
            new Velocity(Vector2.Zero)
        );

        return entity;
    }
}

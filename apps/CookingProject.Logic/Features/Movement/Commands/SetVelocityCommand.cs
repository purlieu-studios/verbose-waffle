using Arch.Core;
using CookingProject.Logic.Core.Commands;
using CookingProject.Logic.Core.Math;

namespace CookingProject.Logic.Features.Movement.Commands;

/// <summary>
/// Command to set an entity's velocity.
/// </summary>
public record SetVelocityCommand(Entity Entity, Vector2 Velocity) : IGameCommand;

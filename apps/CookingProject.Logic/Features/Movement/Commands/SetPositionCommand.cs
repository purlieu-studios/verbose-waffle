using Arch.Core;
using CookingProject.Logic.Core.Commands;
using CookingProject.Logic.Core.Math;

namespace CookingProject.Logic.Features.Movement.Commands;

/// <summary>
/// Command to set an entity's position directly.
/// </summary>
public record SetPositionCommand(Entity Entity, Vector2 Position) : IGameCommand;

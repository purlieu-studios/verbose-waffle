using Arch.Core;

namespace CookingProject.Logic.Commands;

/// <summary>
/// Command to adjust the heat level of a cooking entity.
/// </summary>
/// <param name="Entity">The ECS entity of the cooking station/stove.</param>
/// <param name="HeatLevel">The new heat level (0.0 to 1.0).</param>
public record AdjustHeatCommand(Entity Entity, float HeatLevel) : IGameCommand;

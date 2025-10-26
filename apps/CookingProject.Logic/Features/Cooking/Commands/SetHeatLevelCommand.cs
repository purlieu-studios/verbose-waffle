using Arch.Core;
using CookingProject.Logic.Core.Commands;

namespace CookingProject.Logic.Features.Cooking.Commands;

/// <summary>
/// Command to adjust the heat level of a burner.
/// Heat levels: 0.0 (Off), 0.33 (Low), 0.66 (Medium), 1.0 (High).
/// </summary>
/// <param name="BurnerEntity">The ECS entity of the burner to adjust.</param>
/// <param name="NewHeatLevel">The target heat level (0.0, 0.33, 0.66, or 1.0).</param>
public record SetHeatLevelCommand(Entity BurnerEntity, float NewHeatLevel) : IGameCommand;

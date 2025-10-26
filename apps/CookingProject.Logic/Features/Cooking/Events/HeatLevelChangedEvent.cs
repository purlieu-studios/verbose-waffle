using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Cooking.Events;

/// <summary>
/// Event emitted when a burner's heat level is changed.
/// </summary>
public record HeatLevelChangedEvent(int BurnerEntityId, float NewHeatLevel) : IGameEvent;

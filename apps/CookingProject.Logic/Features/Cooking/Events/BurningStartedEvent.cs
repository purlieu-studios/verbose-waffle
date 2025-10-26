using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Cooking.Events;

/// <summary>
/// Event emitted when food starts burning due to excessive heat.
/// </summary>
public record BurningStartedEvent(int FoodEntityId) : IGameEvent;

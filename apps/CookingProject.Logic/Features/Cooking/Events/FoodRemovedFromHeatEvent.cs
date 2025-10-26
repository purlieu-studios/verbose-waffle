using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Cooking.Events;

/// <summary>
/// Event emitted when food is removed from a heat source and starts cooling.
/// </summary>
public record FoodRemovedFromHeatEvent(int FoodEntityId) : IGameEvent;

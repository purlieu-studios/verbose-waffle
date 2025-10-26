using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Cooking.Events;

/// <summary>
/// Event emitted when food is placed on a heat source.
/// </summary>
public record FoodPlacedOnHeatEvent(int FoodEntityId, int BurnerEntityId) : IGameEvent;

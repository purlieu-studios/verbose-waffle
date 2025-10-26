using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Cooking.Events;

/// <summary>
/// Event emitted when food reaches perfect doneness (1.0).
/// </summary>
public record FoodCookedEvent(
    int FoodEntityId,
    float FinalDoneness,
    float Quality) : IGameEvent;

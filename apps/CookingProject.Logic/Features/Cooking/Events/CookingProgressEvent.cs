using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Cooking.Events;

/// <summary>
/// Event emitted during cooking to update UI/feedback systems.
/// Emitted every frame while food is cooking or cooling.
/// </summary>
public record CookingProgressEvent(
    int FoodEntityId,
    float Doneness,
    bool IsInOptimalRange) : IGameEvent;

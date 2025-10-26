namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired when player begins sharpening a knife.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the knife.</param>
/// <param name="Duration">The sharpening duration in seconds.</param>
public record SharpeningStartedEvent(int EntityId, float Duration) : IGameEvent;

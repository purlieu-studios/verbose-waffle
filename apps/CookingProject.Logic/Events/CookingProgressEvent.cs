namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired to update cooking progress (for UI progress bars, etc.).
/// </summary>
/// <param name="EntityId">The ECS entity ID of the cooking item.</param>
/// <param name="Progress">Progress from 0.0 (raw) to 1.0 (done).</param>
/// <param name="Temperature">Current temperature.</param>
public record CookingProgressEvent(int EntityId, float Progress, float Temperature) : IGameEvent;

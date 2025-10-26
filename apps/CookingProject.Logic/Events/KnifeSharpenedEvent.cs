namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired when sharpening completes successfully.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the knife.</param>
/// <param name="FinalSharpness">The final sharpness level (usually MaxLevel).</param>
public record KnifeSharpenedEvent(int EntityId, float FinalSharpness) : IGameEvent;

namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired when knife sharpness degrades from use (chopping).
/// </summary>
/// <param name="EntityId">The ECS entity ID of the knife.</param>
/// <param name="NewSharpness">The new sharpness level after degradation.</param>
public record KnifeDegradedEvent(int EntityId, float NewSharpness) : IGameEvent;

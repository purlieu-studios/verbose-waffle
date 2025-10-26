using CookingProject.Logic.Core.Events;
namespace CookingProject.Logic.Features.Sharpening.Events;

/// <summary>
/// Event fired every frame during sharpening to update UI.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the knife.</param>
/// <param name="Progress">Progress from 0.0 to 1.0.</param>
public record SharpeningProgressEvent(int EntityId, float Progress) : IGameEvent;

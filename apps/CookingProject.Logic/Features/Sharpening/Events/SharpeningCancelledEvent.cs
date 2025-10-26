using CookingProject.Logic.Core.Events;
namespace CookingProject.Logic.Features.Sharpening.Events;

/// <summary>
/// Event fired when player cancels sharpening mid-process.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the knife.</param>
/// <param name="PartialProgress">How far through sharpening they were (0.0 to 1.0).</param>
public record SharpeningCancelledEvent(int EntityId, float PartialProgress) : IGameEvent;

using Arch.Core;

namespace CookingProject.Logic.Commands;

/// <summary>
/// Command to cancel active sharpening.
/// Partial progress is lost - player must start over.
/// </summary>
/// <param name="KnifeEntity">The ECS entity of the knife being sharpened.</param>
public record CancelSharpeningCommand(Entity KnifeEntity) : IGameCommand;

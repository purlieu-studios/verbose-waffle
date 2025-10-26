using Arch.Core;
using CookingProject.Logic.Core.Commands;

namespace CookingProject.Logic.Features.Sharpening.Commands;

/// <summary>
/// Command to start sharpening a knife.
/// Player must interact with sharpening stone for the specified duration.
/// </summary>
/// <param name="KnifeEntity">The ECS entity of the knife to sharpen.</param>
/// <param name="Duration">Sharpening duration in seconds (default 5.0).</param>
public record StartSharpeningCommand(Entity KnifeEntity, float Duration) : IGameCommand;

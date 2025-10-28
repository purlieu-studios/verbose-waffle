using Arch.Core;
using CookingProject.Logic.Core.Commands;

namespace CookingProject.Logic.Features.Chopping.Commands;

/// <summary>
/// Command to cancel an active chopping operation.
/// Removes ChoppingProgress component without completing the chop or degrading the knife.
/// </summary>
public record CancelChoppingCommand(Entity IngredientEntity) : IGameCommand;

using Arch.Core;
using CookingProject.Logic.Core.Commands;

namespace CookingProject.Logic.Features.Chopping.Commands;

/// <summary>
/// Command to start chopping an ingredient with a knife.
/// Validates that both ingredient and knife exist and have required components.
/// </summary>
public record StartChoppingCommand(Entity IngredientEntity, Entity KnifeEntity) : IGameCommand;

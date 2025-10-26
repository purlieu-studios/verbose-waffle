using Arch.Core;

namespace CookingProject.Logic.Commands;

/// <summary>
/// Command to chop an ingredient entity.
/// </summary>
/// <param name="IngredientEntity">The ECS entity of the ingredient to chop.</param>
public record ChopIngredientCommand(Entity IngredientEntity) : IGameCommand;

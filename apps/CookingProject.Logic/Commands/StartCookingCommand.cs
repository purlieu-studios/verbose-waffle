using Arch.Core;

namespace CookingProject.Logic.Commands;

/// <summary>
/// Command to start cooking a recipe.
/// </summary>
/// <param name="RecipeEntity">The ECS entity of the recipe to cook.</param>
/// <param name="HeatLevel">The heat level to use (0.0 to 1.0).</param>
public record StartCookingCommand(Entity RecipeEntity, float HeatLevel) : IGameCommand;

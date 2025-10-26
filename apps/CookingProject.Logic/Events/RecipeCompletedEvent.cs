namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired when a recipe has been successfully completed.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the recipe.</param>
/// <param name="RecipeName">The name of the completed recipe.</param>
/// <param name="Score">The score earned for completing the recipe.</param>
public record RecipeCompletedEvent(int EntityId, string RecipeName, int Score) : IGameEvent;

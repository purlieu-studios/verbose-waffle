namespace CookingProject.Logic.Components;

/// <summary>
/// Component representing a recipe that can be cooked.
/// </summary>
public struct Recipe
{
    /// <summary>
    /// The name of the recipe (e.g., "Tomato Soup").
    /// </summary>
    public string Name;

    /// <summary>
    /// List of required ingredient names.
    /// </summary>
    public string[] RequiredIngredients;

    /// <summary>
    /// Whether this recipe has been completed.
    /// </summary>
    public bool IsComplete;

    /// <summary>
    /// Score earned for completing this recipe.
    /// </summary>
    public int Score;
}

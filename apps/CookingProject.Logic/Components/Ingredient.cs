namespace CookingProject.Logic.Components;

/// <summary>
/// Component representing an ingredient in the game.
/// </summary>
public struct Ingredient
{
    /// <summary>
    /// The name of the ingredient (e.g., "Tomato", "Onion").
    /// </summary>
    public string Name;

    /// <summary>
    /// Whether the ingredient has been chopped/prepared.
    /// </summary>
    public bool IsChopped;
}

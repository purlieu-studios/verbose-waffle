namespace CookingProject.Logic.Features.Chopping.Components;

/// <summary>
/// Hardness level of an ingredient.
/// Determines chopping speed and knife degradation.
/// </summary>
public enum IngredientHardness
{
    /// <summary>
    /// Soft ingredients (tomatoes, herbs) - fast to chop, low degradation.
    /// </summary>
    Soft = 0,

    /// <summary>
    /// Medium ingredients (peppers, cucumbers) - moderate speed and degradation.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Hard ingredients (carrots, potatoes) - slow to chop, high degradation.
    /// </summary>
    Hard = 2,
}

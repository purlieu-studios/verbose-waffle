namespace CookingProject.Logic.Features.Chopping.Components;

/// <summary>
/// Defines the properties of a food ingredient.
/// Determines chopping time, knife degradation, and preparation requirements.
/// </summary>
public struct Ingredient
{
    /// <summary>
    /// Type of ingredient (Carrot, Tomato, etc.).
    /// </summary>
    public IngredientType Type;

    /// <summary>
    /// Hardness level affecting chop time and knife wear.
    /// </summary>
    public IngredientHardness Hardness;

    /// <summary>
    /// Base time to complete one chop at 1.0 knife sharpness (in seconds).
    /// Actual time varies based on knife sharpness.
    /// </summary>
    public float BaseChopTime;

    /// <summary>
    /// Amount of sharpness degradation per chop.
    /// Applied to knife when chop completes.
    /// </summary>
    public float DegradationAmount;

    public Ingredient(IngredientType type, IngredientHardness hardness, float baseChopTime, float degradationAmount)
    {
        Type = type;
        Hardness = hardness;
        BaseChopTime = baseChopTime;
        DegradationAmount = degradationAmount;
    }

    /// <summary>
    /// Creates a hard ingredient (carrot, potato, onion).
    /// </summary>
    public static Ingredient CreateHard(IngredientType type)
    {
        return new Ingredient(type, IngredientHardness.Hard, 2.0f, 0.08f);
    }

    /// <summary>
    /// Creates a medium ingredient (pepper, cucumber, zucchini).
    /// </summary>
    public static Ingredient CreateMedium(IngredientType type)
    {
        return new Ingredient(type, IngredientHardness.Medium, 1.2f, 0.05f);
    }

    /// <summary>
    /// Creates a soft ingredient (tomato, mushroom, herbs).
    /// </summary>
    public static Ingredient CreateSoft(IngredientType type)
    {
        return new Ingredient(type, IngredientHardness.Soft, 0.8f, 0.03f);
    }
}

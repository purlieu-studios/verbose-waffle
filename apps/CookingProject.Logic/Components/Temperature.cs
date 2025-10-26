namespace CookingProject.Logic.Components;

/// <summary>
/// Component representing temperature for cooking.
/// </summary>
public struct Temperature
{
    /// <summary>
    /// Current temperature in degrees.
    /// </summary>
    public float Current;

    /// <summary>
    /// Target/ideal temperature for this item.
    /// </summary>
    public float Target;

    /// <summary>
    /// Heat level being applied (0.0 to 1.0).
    /// </summary>
    public float HeatLevel;
}

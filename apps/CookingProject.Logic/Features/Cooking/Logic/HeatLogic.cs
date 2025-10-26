namespace CookingProject.Logic.Features.Cooking.Logic;

/// <summary>
/// Pure business logic for heat source management with no ECS dependencies.
/// All methods are pure functions that can be tested independently.
/// </summary>
public static class HeatLogic
{
    /// <summary>
    /// Valid heat level values for burners.
    /// </summary>
    public const float HeatOff = 0.0f;
    public const float HeatLow = 0.33f;
    public const float HeatMedium = 0.66f;
    public const float HeatHigh = 1.0f;

    /// <summary>
    /// Validates that a heat level is one of the allowed discrete values.
    /// </summary>
    /// <param name="heatLevel">The heat level to validate.</param>
    /// <returns>True if the heat level is valid (0.0, 0.33, 0.66, or 1.0).</returns>
    public static bool IsValidHeatLevel(float heatLevel)
    {
        return heatLevel == HeatOff ||
               Math.Abs(heatLevel - HeatLow) < 0.01f ||
               Math.Abs(heatLevel - HeatMedium) < 0.01f ||
               Math.Abs(heatLevel - HeatHigh) < 0.01f;
    }

    /// <summary>
    /// Snaps a heat level to the nearest valid discrete value.
    /// </summary>
    /// <param name="heatLevel">The heat level to snap.</param>
    /// <returns>The nearest valid heat level (0.0, 0.33, 0.66, or 1.0).</returns>
    public static float SnapToValidHeatLevel(float heatLevel)
    {
        if (heatLevel <= (HeatOff + HeatLow) / 2) return HeatOff;
        if (heatLevel <= (HeatLow + HeatMedium) / 2) return HeatLow;
        if (heatLevel <= (HeatMedium + HeatHigh) / 2) return HeatMedium;
        return HeatHigh;
    }

    /// <summary>
    /// Determines if the current heat level is within the optimal range for cooking.
    /// </summary>
    /// <param name="currentHeat">Current heat level of the burner.</param>
    /// <param name="optimalMin">Minimum optimal heat level.</param>
    /// <param name="optimalMax">Maximum optimal heat level.</param>
    /// <returns>True if heat is in optimal range.</returns>
    public static bool IsHeatInOptimalRange(float currentHeat, float optimalMin, float optimalMax)
    {
        return currentHeat >= optimalMin && currentHeat <= optimalMax;
    }
}

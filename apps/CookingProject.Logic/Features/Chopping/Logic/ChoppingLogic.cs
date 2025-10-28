using System;

namespace CookingProject.Logic.Features.Chopping.Logic;

/// <summary>
/// Pure functions for chopping calculations.
/// No side effects, no ECS dependencies - testable logic only.
/// </summary>
public static class ChoppingLogic
{
    /// <summary>
    /// Calculates actual chop time based on ingredient base time and knife sharpness.
    /// Formula: chopTime = baseChopTime / (0.3 + (sharpness * 0.7))
    /// </summary>
    /// <param name="baseChopTime">Ingredient's base chop time at 1.0 sharpness (seconds).</param>
    /// <param name="sharpness">Knife sharpness level (0.0 to 1.0+).</param>
    /// <returns>Actual time required to complete one chop (seconds).</returns>
    public static float CalculateChopTime(float baseChopTime, float sharpness)
    {
        // Ensure sharpness is non-negative
        float clampedSharpness = Math.Max(0f, sharpness);

        // Formula ensures even dull knives work (0.3 minimum multiplier)
        // Sharp knives get full speed (1.0 multiplier)
        float speedMultiplier = 0.3f + (clampedSharpness * 0.7f);

        return baseChopTime / speedMultiplier;
    }

    /// <summary>
    /// Calculates progress percentage for current chop.
    /// </summary>
    /// <param name="elapsedTime">Time spent chopping (seconds).</param>
    /// <param name="chopDuration">Total time required (seconds).</param>
    /// <returns>Progress from 0.0 to 1.0.</returns>
    public static float CalculateProgress(float elapsedTime, float chopDuration)
    {
        if (chopDuration <= 0f)
        {
            return 1.0f; // Instant completion if duration is zero
        }

        float progress = elapsedTime / chopDuration;
        return Math.Clamp(progress, 0f, 1f);
    }

    /// <summary>
    /// Checks if a chop should complete.
    /// </summary>
    /// <param name="elapsedTime">Time spent chopping (seconds).</param>
    /// <param name="chopDuration">Total time required (seconds).</param>
    /// <returns>True if chop is complete.</returns>
    public static bool ShouldCompleteChop(float elapsedTime, float chopDuration)
    {
        return elapsedTime >= chopDuration;
    }

    /// <summary>
    /// Checks if an ingredient is fully prepared (all chops complete).
    /// </summary>
    /// <param name="currentChops">Chops completed so far.</param>
    /// <param name="requiredChops">Total chops needed.</param>
    /// <returns>True if ingredient is fully chopped.</returns>
    public static bool IsFullyChopped(int currentChops, int requiredChops)
    {
        return currentChops >= requiredChops;
    }

    /// <summary>
    /// Applies knife degradation, clamping to minimum of 0.0.
    /// </summary>
    /// <param name="currentSharpness">Current knife sharpness.</param>
    /// <param name="degradationAmount">Amount to degrade.</param>
    /// <returns>New sharpness level (cannot go below 0.0).</returns>
    public static float ApplyDegradation(float currentSharpness, float degradationAmount)
    {
        float newSharpness = currentSharpness - degradationAmount;
        return Math.Max(0f, newSharpness);
    }

    /// <summary>
    /// Advances chop count, clamping to required chops maximum.
    /// </summary>
    /// <param name="currentChops">Current number of chops.</param>
    /// <param name="requiredChops">Maximum chops allowed.</param>
    /// <returns>New chop count (cannot exceed required).</returns>
    public static int IncrementChops(int currentChops, int requiredChops)
    {
        int newChops = currentChops + 1;
        return Math.Min(newChops, requiredChops);
    }
}

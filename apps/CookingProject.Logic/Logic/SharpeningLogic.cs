namespace CookingProject.Logic.Logic;

/// <summary>
/// Pure business logic for knife sharpening with no ECS dependencies.
/// All methods are pure functions that can be tested independently.
/// </summary>
public static class SharpeningLogic
{
    /// <summary>
    /// Calculates how much sharpness to restore this frame.
    /// Uses initial gap to maintain constant sharpening rate.
    /// </summary>
    /// <param name="initialLevel">Sharpness level when sharpening started (0.0-MaxLevel).</param>
    /// <param name="maxLevel">Maximum sharpness level (e.g., 1.0, 1.5 with upgrades).</param>
    /// <param name="duration">Total sharpening duration in seconds.</param>
    /// <param name="deltaTime">Time elapsed this frame in seconds.</param>
    /// <returns>Amount of sharpness to add this frame.</returns>
    public static float CalculateSharpenAmount(float initialLevel, float maxLevel, float duration, float deltaTime)
    {
        float totalGap = maxLevel - initialLevel;
        float ratePerSecond = totalGap / duration;
        return ratePerSecond * deltaTime;
    }

    /// <summary>
    /// Applies sharpening progress, clamping to max level.
    /// </summary>
    /// <param name="currentLevel">Current sharpness level.</param>
    /// <param name="sharpenAmount">Amount to add.</param>
    /// <param name="maxLevel">Maximum sharpness level.</param>
    /// <returns>New sharpness level, clamped to maxLevel.</returns>
    public static float ApplySharpeningProgress(float currentLevel, float sharpenAmount, float maxLevel)
    {
        float newLevel = currentLevel + sharpenAmount;
        return Math.Min(newLevel, maxLevel);
    }

    /// <summary>
    /// Checks if sharpening is complete.
    /// </summary>
    /// <param name="elapsedTime">Time spent sharpening so far.</param>
    /// <param name="duration">Total required sharpening time.</param>
    /// <returns>True if sharpening is complete.</returns>
    public static bool IsComplete(float elapsedTime, float duration)
    {
        return elapsedTime >= duration;
    }

    /// <summary>
    /// Calculates progress percentage for UI display.
    /// </summary>
    /// <param name="elapsedTime">Time spent sharpening.</param>
    /// <param name="duration">Total required time.</param>
    /// <returns>Progress from 0.0 to 1.0.</returns>
    public static float CalculateProgressPercent(float elapsedTime, float duration)
    {
        return duration > 0 ? elapsedTime / duration : 1.0f;
    }
}

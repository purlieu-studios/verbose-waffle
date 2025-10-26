namespace CookingProject.Logic.Features.Cooking.Logic;

/// <summary>
/// Pure business logic for cooking mechanics with no ECS dependencies.
/// All methods are pure functions that can be tested independently.
/// </summary>
public static class CookingLogic
{
    /// <summary>
    /// Doneness level at which food is perfectly cooked.
    /// </summary>
    public const float PerfectDoneness = 1.0f;

    /// <summary>
    /// Doneness level at which food is completely burnt/ruined.
    /// </summary>
    public const float MaxDoneness = 2.0f;

    /// <summary>
    /// Cooling rate as a percentage of normal cooking speed (0.3 = 30%).
    /// </summary>
    public const float CoolingRateMultiplier = 0.3f;

    /// <summary>
    /// Calculates how much doneness to add this frame based on heat level.
    /// </summary>
    /// <param name="currentHeat">Current heat level (0.0 to 1.0).</param>
    /// <param name="cookTimeSeconds">Total time to reach perfect doneness at optimal heat.</param>
    /// <param name="deltaTime">Time elapsed this frame in seconds.</param>
    /// <returns>Amount of doneness to add this frame.</returns>
    public static float CalculateCookingProgress(float currentHeat, float cookTimeSeconds, float deltaTime)
    {
        if (currentHeat <= 0.0f || cookTimeSeconds <= 0.0f)
            return 0.0f;

        // Heat directly affects cooking speed (higher heat = faster cooking)
        float ratePerSecond = currentHeat / cookTimeSeconds;
        return ratePerSecond * deltaTime;
    }

    /// <summary>
    /// Calculates how much doneness to remove this frame when cooling.
    /// </summary>
    /// <param name="cookTimeSeconds">Total time to reach perfect doneness.</param>
    /// <param name="deltaTime">Time elapsed this frame in seconds.</param>
    /// <returns>Amount of doneness to subtract this frame (positive value).</returns>
    public static float CalculateCoolingProgress(float cookTimeSeconds, float deltaTime)
    {
        if (cookTimeSeconds <= 0.0f)
            return 0.0f;

        // Cooling happens at 30% of the normal cooking rate
        float baseRatePerSecond = 1.0f / cookTimeSeconds;
        float coolingRate = baseRatePerSecond * CoolingRateMultiplier;
        return coolingRate * deltaTime;
    }

    /// <summary>
    /// Applies cooking or cooling progress to the current doneness level.
    /// </summary>
    /// <param name="currentDoneness">Current doneness level.</param>
    /// <param name="progressAmount">Amount to change (positive for cooking, negative for cooling).</param>
    /// <returns>New doneness level, clamped between 0.0 and MaxDoneness.</returns>
    public static float ApplyDonenessChange(float currentDoneness, float progressAmount)
    {
        float newDoneness = currentDoneness + progressAmount;
        return Math.Clamp(newDoneness, 0.0f, MaxDoneness);
    }

    /// <summary>
    /// Checks if food has reached perfect doneness.
    /// </summary>
    /// <param name="doneness">Current doneness level.</param>
    /// <returns>True if food is perfectly cooked (doneness >= 1.0).</returns>
    public static bool IsPerfectlyCooked(float doneness)
    {
        return doneness >= PerfectDoneness;
    }

    /// <summary>
    /// Checks if food has started burning (exceeded perfect doneness).
    /// </summary>
    /// <param name="doneness">Current doneness level.</param>
    /// <returns>True if food is burning (doneness > 1.0).</returns>
    public static bool IsBurning(float doneness)
    {
        return doneness > PerfectDoneness;
    }

    /// <summary>
    /// Calculates burn progress when food is overcooked.
    /// Burn level goes from 0.0 (just started burning) to 1.0 (completely ruined).
    /// </summary>
    /// <param name="doneness">Current doneness level.</param>
    /// <returns>Burn level from 0.0 to 1.0, or 0.0 if not burning.</returns>
    public static float CalculateBurnLevel(float doneness)
    {
        if (doneness <= PerfectDoneness)
            return 0.0f;

        // Map doneness from 1.0→2.0 to burn level 0.0→1.0
        float burnProgress = doneness - PerfectDoneness;
        float maxBurnProgress = MaxDoneness - PerfectDoneness;
        return Math.Clamp(burnProgress / maxBurnProgress, 0.0f, 1.0f);
    }

    /// <summary>
    /// Calculates cooking quality based on final doneness.
    /// Perfect doneness (1.0) yields quality of 1.0.
    /// Quality degrades as food is undercooked or overcooked.
    /// </summary>
    /// <param name="doneness">Final doneness level.</param>
    /// <returns>Quality from 0.0 (ruined) to 1.0 (perfect).</returns>
    public static float CalculateQuality(float doneness)
    {
        // Perfect at 1.0, degrades towards 0.0 or 2.0
        float distanceFromPerfect = Math.Abs(doneness - PerfectDoneness);
        float maxDistance = Math.Max(PerfectDoneness, MaxDoneness - PerfectDoneness);

        float quality = 1.0f - (distanceFromPerfect / maxDistance);
        return Math.Clamp(quality, 0.0f, 1.0f);
    }

    /// <summary>
    /// Calculates progress percentage for UI display.
    /// </summary>
    /// <param name="doneness">Current doneness level.</param>
    /// <returns>Progress from 0.0 (raw) to 1.0 (perfect).</returns>
    public static float CalculateProgressPercent(float doneness)
    {
        return Math.Clamp(doneness / PerfectDoneness, 0.0f, 1.0f);
    }
}

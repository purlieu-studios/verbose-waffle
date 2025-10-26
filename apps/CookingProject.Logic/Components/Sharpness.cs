namespace CookingProject.Logic.Components;

/// <summary>
/// Component tracking knife sharpness level.
/// Sharpness affects chopping speed and degrades with use.
/// </summary>
public struct Sharpness
{
    /// <summary>
    /// Current sharpness level from 0.0 (completely dull) to 1.0 (razor sharp).
    /// At 0.0, knife is 3.33x slower than at 1.0.
    /// </summary>
    public float Level;

    /// <summary>
    /// Maximum sharpness achievable (default 1.0, upgradeable to 1.2, 1.5).
    /// Sharpening restores to this value.
    /// </summary>
    public float MaxLevel;
}

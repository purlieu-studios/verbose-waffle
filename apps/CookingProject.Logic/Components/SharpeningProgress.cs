namespace CookingProject.Logic.Components;

/// <summary>
/// Component tracking active sharpening progress.
/// Presence of this component indicates the knife is being sharpened.
/// Removed when sharpening completes or is cancelled.
/// </summary>
public struct SharpeningProgress
{
    /// <summary>
    /// Sharpness level when sharpening started.
    /// Used to maintain constant sharpening rate.
    /// </summary>
    public float InitialLevel;

    /// <summary>
    /// Time spent sharpening in seconds.
    /// </summary>
    public float ElapsedTime;

    /// <summary>
    /// Total sharpening duration in seconds (default 5.0).
    /// Can be reduced via upgrades (4s -> 3s -> 2s).
    /// </summary>
    public float Duration;
}

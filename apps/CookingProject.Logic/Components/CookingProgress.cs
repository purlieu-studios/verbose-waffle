namespace CookingProject.Logic.Components;

/// <summary>
/// Component tracking cooking progress.
/// </summary>
public struct CookingProgress
{
    /// <summary>
    /// Cooking progress from 0.0 (raw) to 1.0 (done).
    /// </summary>
    public float Progress;

    /// <summary>
    /// Time spent cooking in seconds.
    /// </summary>
    public float TimeCooked;

    /// <summary>
    /// Required cooking time in seconds.
    /// </summary>
    public float RequiredTime;

    /// <summary>
    /// Whether the item is currently being cooked.
    /// </summary>
    public bool IsCooking;
}

namespace CookingProject.Logic.Features.Cooking.Components;

/// <summary>
/// Component tracking burn damage to food.
/// Added when food starts burning (heat too high).
/// </summary>
public struct BurnProgress
{
    /// <summary>
    /// Current burn level.
    /// 0.0 = no burn damage
    /// 1.0 = completely ruined
    /// </summary>
    public float BurnLevel;
}

using Arch.Core;

namespace CookingProject.Logic.Features.Cooking.Components;

/// <summary>
/// Component tracking cooking progress of a food item.
/// Added when food is placed on heat, removed when cooking complete or food ruined.
/// </summary>
public struct CookingProgress
{
    /// <summary>
    /// Current doneness level.
    /// 0.0 = raw
    /// 1.0 = perfectly cooked
    /// 2.0 = completely burnt/ruined
    /// </summary>
    public float Doneness;

    /// <summary>
    /// Minimum heat level for optimal cooking (e.g., 0.5 for medium).
    /// Below this, cooking is slow.
    /// </summary>
    public float OptimalHeatMin;

    /// <summary>
    /// Maximum heat level for optimal cooking (e.g., 0.8 for medium-high).
    /// Above this, food burns while cooking.
    /// </summary>
    public float OptimalHeatMax;

    /// <summary>
    /// Time in seconds needed to cook from raw to done at optimal heat.
    /// </summary>
    public float CookTimeSeconds;

    /// <summary>
    /// Whether food is currently on a heat source.
    /// If false, food is cooling down (doneness decreasing).
    /// </summary>
    public bool IsOnHeat;

    /// <summary>
    /// The stove/burner entity this food is on.
    /// Only valid if IsOnHeat is true.
    /// </summary>
    public Entity StoveEntity;
}

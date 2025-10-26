namespace CookingProject.Logic.Features.Cooking.Components;

/// <summary>
/// Component defining how a food item should be cooked.
/// Attached to food/ingredient entities.
/// </summary>
public struct CookingRequirements
{
    /// <summary>
    /// Whether this food must be placed in a container (pan/pot) before cooking.
    /// Examples: Eggs need pan (true), Steak can go direct on grill (false).
    /// </summary>
    public bool RequiresContainer;

    /// <summary>
    /// Type of container required if RequiresContainer is true.
    /// </summary>
    public ContainerType RequiredContainerType;

    /// <summary>
    /// Minimum heat level for optimal cooking.
    /// Below this, food cooks slowly.
    /// </summary>
    public float OptimalHeatMin;

    /// <summary>
    /// Maximum heat level for optimal cooking.
    /// Above this, food burns while cooking.
    /// </summary>
    public float OptimalHeatMax;

    /// <summary>
    /// Time in seconds to cook from raw to done at optimal heat.
    /// </summary>
    public float CookTimeSeconds;
}

namespace CookingProject.Logic.Features.Cooking.Components;

/// <summary>
/// Types of cooking containers.
/// </summary>
public enum ContainerType
{
    Pan,
    Pot
}

/// <summary>
/// Component for cooking container entities (pans, pots).
/// Containers can hold food and be placed on heat sources.
/// </summary>
public struct Container
{
    /// <summary>
    /// Type of container (Pan or Pot).
    /// </summary>
    public ContainerType Type;

    /// <summary>
    /// Entity ID of food currently in this container.
    /// Null if container is empty.
    /// </summary>
    public int? ContainingFoodId;
}

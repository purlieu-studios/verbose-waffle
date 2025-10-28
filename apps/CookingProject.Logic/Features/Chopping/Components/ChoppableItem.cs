namespace CookingProject.Logic.Features.Chopping.Components;

/// <summary>
/// Tracks chopping progress for an ingredient.
/// Entities with this component can be chopped multiple times until fully prepared.
/// </summary>
public struct ChoppableItem
{
    /// <summary>
    /// Total number of chops required to fully prepare this ingredient.
    /// Defined by recipe requirements (e.g., carrot needs 4 chops for diced).
    /// </summary>
    public int RequiredChops;

    /// <summary>
    /// Number of chops completed so far (0 to RequiredChops).
    /// </summary>
    public int CurrentChops;

    /// <summary>
    /// True when CurrentChops >= RequiredChops.
    /// Only fully chopped ingredients can be cooked.
    /// </summary>
    public bool IsFullyChopped;

    public ChoppableItem(int requiredChops)
    {
        RequiredChops = requiredChops;
        CurrentChops = 0;
        IsFullyChopped = false;
    }
}

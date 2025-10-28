using CookingProject.Logic.Core.Components;
using CookingProject.Logic.Features.Chopping.Components;
using CookingProject.Logic.Features.Cooking.Components;
using CookingProject.Logic.Features.Movement.Components;
using CookingProject.Logic.Features.Sharpening.Components;

namespace CookingProject.Logic.Debug;

/// <summary>
/// Registry of all known component types for debug introspection.
/// Add new component types here as they're created.
/// </summary>
public static class ComponentRegistry
{
    /// <summary>
    /// All known component types in the game.
    /// This is used by the debug system to serialize component values.
    /// </summary>
    private static readonly Type[] s_allComponentTypes = new[]
    {
        // Core components
        typeof(Tool),

        // Movement components
        typeof(Position),
        typeof(Velocity),

        // Sharpening components
        typeof(Sharpness),
        typeof(SharpeningProgress),

        // Cooking components
        typeof(HeatSource),
        typeof(CookingProgress),
        typeof(CookingRequirements),
        typeof(BurnProgress),
        typeof(Container),

        // Chopping components
        typeof(Ingredient),
        typeof(ChoppableItem),
        typeof(ChoppingProgress)
    };

    /// <summary>
    /// Gets all registered component types.
    /// </summary>
    public static Type[] GetAllComponentTypes() => s_allComponentTypes;
}

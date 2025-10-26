using Arch.Core;

namespace CookingProject.Logic.Features.Cooking.Components;

/// <summary>
/// Component representing a heat source (burner/stove).
/// Discrete heat levels: Off (0.0), Low (0.33), Medium (0.66), High (1.0).
/// Heat stays at the set level without drift.
/// </summary>
public struct HeatSource
{
    /// <summary>
    /// Current heat level. One of: 0.0 (Off), 0.33 (Low), 0.66 (Medium), 1.0 (High).
    /// </summary>
    public float CurrentHeat;

    /// <summary>
    /// The entity currently on this burner (food or container entity).
    /// Check HasCookingEntity to see if burner is occupied.
    /// </summary>
    public Entity CookingEntity;

    /// <summary>
    /// Whether this burner currently has food on it.
    /// </summary>
    public bool HasCookingEntity;
}

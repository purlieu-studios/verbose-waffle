using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Chopping.Events;

/// <summary>
/// Event emitted when an ingredient reaches the required number of chops.
/// Godot should play completion sound, enable pickup/cooking, and show visual indicator.
/// </summary>
public record IngredientFullyPreparedEvent(int IngredientId) : IGameEvent;

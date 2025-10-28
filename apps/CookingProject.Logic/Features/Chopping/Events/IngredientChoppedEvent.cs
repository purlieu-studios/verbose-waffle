using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Chopping.Events;

/// <summary>
/// Event emitted when a single chop completes successfully.
/// Godot should play chop sound, update ingredient sprite, and trigger particle effects.
/// </summary>
public record IngredientChoppedEvent(int IngredientId, int CurrentChops, bool FullyChopped) : IGameEvent;

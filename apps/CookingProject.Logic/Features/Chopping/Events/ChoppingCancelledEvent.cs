using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Chopping.Events;

/// <summary>
/// Event emitted when the player cancels chopping mid-progress.
/// Godot should hide progress UI and stop animation/sound.
/// </summary>
public record ChoppingCancelledEvent(int IngredientId, float PartialProgress) : IGameEvent;

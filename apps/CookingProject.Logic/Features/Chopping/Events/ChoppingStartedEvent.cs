using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Chopping.Events;

/// <summary>
/// Event emitted when a player begins chopping an ingredient.
/// Godot should show progress UI and play chopping animation/sound.
/// </summary>
public record ChoppingStartedEvent(int IngredientId, int KnifeId, float Duration) : IGameEvent;

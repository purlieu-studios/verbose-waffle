using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Features.Chopping.Events;

/// <summary>
/// Event emitted every frame while chopping is in progress.
/// Godot should update progress bar to reflect current completion percentage.
/// </summary>
public record ChoppingProgressEvent(int IngredientId, float Progress) : IGameEvent;

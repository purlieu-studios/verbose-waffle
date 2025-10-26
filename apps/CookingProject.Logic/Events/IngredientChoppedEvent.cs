namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired when an ingredient has been chopped.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the ingredient.</param>
/// <param name="IngredientName">The name of the ingredient that was chopped.</param>
public record IngredientChoppedEvent(int EntityId, string IngredientName) : IGameEvent;

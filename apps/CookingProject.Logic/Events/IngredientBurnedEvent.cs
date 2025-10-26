namespace CookingProject.Logic.Events;

/// <summary>
/// Event fired when an ingredient has been burned due to overcooking.
/// </summary>
/// <param name="EntityId">The ECS entity ID of the burned ingredient.</param>
/// <param name="IngredientName">The name of the ingredient that burned.</param>
public record IngredientBurnedEvent(int EntityId, string IngredientName) : IGameEvent;

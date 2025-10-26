using Arch.Core;
using CookingProject.Logic.Core.Commands;

namespace CookingProject.Logic.Features.Cooking.Commands;

/// <summary>
/// Command to place food on a burner to start cooking.
/// If the food requires a container, it must be in one before placing.
/// </summary>
/// <param name="FoodEntity">The ECS entity of the food to cook.</param>
/// <param name="BurnerEntity">The ECS entity of the burner to place food on.</param>
public record PlaceFoodOnBurnerCommand(Entity FoodEntity, Entity BurnerEntity) : IGameCommand;

using Arch.Core;
using CookingProject.Logic.Core.Commands;

namespace CookingProject.Logic.Features.Cooking.Commands;

/// <summary>
/// Command to remove food from a burner.
/// Food will begin cooling down gradually after removal.
/// </summary>
/// <param name="FoodEntity">The ECS entity of the food to remove.</param>
public record RemoveFoodFromBurnerCommand(Entity FoodEntity) : IGameCommand;

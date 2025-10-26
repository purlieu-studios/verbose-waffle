using CookingProject.Logic.Features.Cooking.Commands;
using CookingProject.Logic.Features.Cooking.Components;
using CookingProject.Logic.Features.Cooking.Events;

namespace CookingProject.Logic;

/// <summary>
/// Cooking feature command handlers for GameFacade.
/// </summary>
public partial class GameFacade
{
    private void HandleSetHeatLevel(SetHeatLevelCommand command)
    {
        var entity = command.BurnerEntity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        if (!_world.Has<HeatSource>(entity))
        {
            return; // Entity doesn't have HeatSource component
        }

        // Validate and snap to nearest valid heat level
        float newHeatLevel = Logic.Features.Cooking.Logic.HeatLogic.SnapToValidHeatLevel(command.NewHeatLevel);

        ref var heatSource = ref _world.Get<HeatSource>(entity);
        heatSource.CurrentHeat = newHeatLevel;

        EmitEvent(new HeatLevelChangedEvent(entity.Id, newHeatLevel));
    }

    private void HandlePlaceFoodOnBurner(PlaceFoodOnBurnerCommand command)
    {
        var foodEntity = command.FoodEntity;
        var burnerEntity = command.BurnerEntity;

        if (!_world.IsAlive(foodEntity) || !_world.IsAlive(burnerEntity))
        {
            return;
        }

        if (!_world.Has<CookingRequirements>(foodEntity))
        {
            return; // Food doesn't have cooking requirements
        }

        if (!_world.Has<HeatSource>(burnerEntity))
        {
            return; // Burner doesn't have HeatSource component
        }

        ref var heatSource = ref _world.Get<HeatSource>(burnerEntity);

        // Check if burner is already occupied
        if (heatSource.HasCookingEntity)
        {
            return; // Burner already has food on it
        }

        ref var requirements = ref _world.Get<CookingRequirements>(foodEntity);

        // Check if food requires a container
        if (requirements.RequiresContainer)
        {
            // TODO: Verify food is in correct container type
            // For now, we'll assume the validation is done in the UI layer
        }

        // Add or update CookingProgress component
        if (_world.Has<CookingProgress>(foodEntity))
        {
            ref var existingProgress = ref _world.Get<CookingProgress>(foodEntity);
            existingProgress.IsOnHeat = true;
            existingProgress.StoveEntity = burnerEntity;
        }
        else
        {
            _world.Add(foodEntity, new CookingProgress
            {
                Doneness = 0.0f,
                OptimalHeatMin = requirements.OptimalHeatMin,
                OptimalHeatMax = requirements.OptimalHeatMax,
                CookTimeSeconds = requirements.CookTimeSeconds,
                IsOnHeat = true,
                StoveEntity = burnerEntity
            });
        }

        // Link burner to food
        heatSource.CookingEntity = foodEntity;
        heatSource.HasCookingEntity = true;

        EmitEvent(new FoodPlacedOnHeatEvent(foodEntity.Id, burnerEntity.Id));
    }

    private void HandleRemoveFoodFromBurner(RemoveFoodFromBurnerCommand command)
    {
        var foodEntity = command.FoodEntity;

        if (!_world.IsAlive(foodEntity))
        {
            return;
        }

        if (!_world.Has<CookingProgress>(foodEntity))
        {
            return; // Food is not being cooked
        }

        ref var progress = ref _world.Get<CookingProgress>(foodEntity);

        if (!progress.IsOnHeat)
        {
            return; // Food is already off heat
        }

        // Unlink from burner
        var stoveEntity = progress.StoveEntity;
        if (_world.IsAlive(stoveEntity) && _world.Has<HeatSource>(stoveEntity))
        {
            ref var heatSource = ref _world.Get<HeatSource>(stoveEntity);
            heatSource.HasCookingEntity = false;
        }

        progress.IsOnHeat = false;

        EmitEvent(new FoodRemovedFromHeatEvent(foodEntity.Id));
    }
}

using Arch.Core;
using Arch.Core.Extensions;
using CookingProject.Logic.Core.Systems;
using CookingProject.Logic.Features.Cooking.Components;
using CookingProject.Logic.Features.Cooking.Events;
using CookingProject.Logic.Features.Cooking.Logic;

namespace CookingProject.Logic.Features.Cooking;

/// <summary>
/// ECS system that orchestrates cooking by calling pure business logic.
/// This is a thin wrapper that handles ECS queries and component updates.
/// All business logic is in HeatLogic and CookingLogic for easy testing.
/// </summary>
public class CookingSystem : IGameSystem
{
    private readonly World _world;
    private readonly GameFacade _facade;
    private readonly QueryDescription _cookingQuery;

    public CookingSystem(World world, GameFacade facade)
    {
        _world = world;
        _facade = facade;

        // Query for entities being cooked
        _cookingQuery = new QueryDescription()
            .WithAll<CookingProgress>();
    }

    public void Update(float deltaTime)
    {
        // Process all food items being cooked or cooling
        _world.Query(in _cookingQuery, (ref Entity entity, ref CookingProgress progress) =>
        {
            float previousDoneness = progress.Doneness;

            if (progress.IsOnHeat)
            {
                // Get heat source and calculate cooking progress
                var stoveEntity = progress.StoveEntity;
                if (_world.IsAlive(stoveEntity) && _world.Has<HeatSource>(stoveEntity))
                {
                    ref var heatSource = ref _world.Get<HeatSource>(stoveEntity);
                        float currentHeat = heatSource.CurrentHeat;

                        // Use pure logic to calculate cooking progress
                        float cookingAmount = CookingLogic.CalculateCookingProgress(
                            currentHeat, progress.CookTimeSeconds, deltaTime);

                        progress.Doneness = CookingLogic.ApplyDonenessChange(
                            progress.Doneness, cookingAmount);

                        // Check if heat is in optimal range
                        bool isInOptimalRange = HeatLogic.IsHeatInOptimalRange(
                            currentHeat, progress.OptimalHeatMin, progress.OptimalHeatMax);

                        // Emit progress event for UI updates
                        _facade.EmitEvent(new CookingProgressEvent(
                            entity.Id, progress.Doneness, isInOptimalRange));

                        // Check if food reached perfect doneness
                        if (!CookingLogic.IsPerfectlyCooked(previousDoneness) &&
                            CookingLogic.IsPerfectlyCooked(progress.Doneness))
                        {
                            float quality = CookingLogic.CalculateQuality(progress.Doneness);
                            _facade.EmitEvent(new FoodCookedEvent(entity.Id, progress.Doneness, quality));
                        }

                        // Check if food started burning
                        if (!CookingLogic.IsBurning(previousDoneness) &&
                            CookingLogic.IsBurning(progress.Doneness))
                        {
                            _facade.EmitEvent(new BurningStartedEvent(entity.Id));

                            // Add BurnProgress component if not already present
                            if (!_world.Has<BurnProgress>(entity))
                            {
                                _world.Add(entity, new BurnProgress { BurnLevel = 0.0f });
                            }
                        }

                        // Update burn level if burning
                        if (_world.Has<BurnProgress>(entity))
                        {
                            ref var burnProgress = ref _world.Get<BurnProgress>(entity);
                            burnProgress.BurnLevel = CookingLogic.CalculateBurnLevel(progress.Doneness);
                        }
                    }
                }
            else
            {
                // Food is off heat, cooling down
                float coolingAmount = CookingLogic.CalculateCoolingProgress(
                    progress.CookTimeSeconds, deltaTime);

                progress.Doneness = CookingLogic.ApplyDonenessChange(
                    progress.Doneness, -coolingAmount); // Negative for cooling

                // Emit progress event
                _facade.EmitEvent(new CookingProgressEvent(
                    entity.Id, progress.Doneness, false));

                // If doneness drops below burning threshold, remove BurnProgress
                if (_world.Has<BurnProgress>(entity) && !CookingLogic.IsBurning(progress.Doneness))
                {
                    _world.Remove<BurnProgress>(entity);
                }
            }
        });
    }
}

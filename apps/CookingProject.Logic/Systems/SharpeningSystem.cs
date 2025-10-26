using Arch.Core;
using Arch.Core.Extensions;
using CookingProject.Logic.Components;
using CookingProject.Logic.Events;
using CookingProject.Logic.Logic;

namespace CookingProject.Logic.Systems;

/// <summary>
/// ECS system that orchestrates knife sharpening by calling pure business logic.
/// This is a thin wrapper that handles ECS queries and component updates.
/// All business logic is in SharpeningLogic for easy testing.
/// </summary>
public class SharpeningSystem : IGameSystem
{
    private readonly World _world;
    private readonly GameFacade _facade;
    private readonly QueryDescription _sharpeningQuery;

    public SharpeningSystem(World world, GameFacade facade)
    {
        _world = world;
        _facade = facade;

        // Query for entities being sharpened
        _sharpeningQuery = new QueryDescription()
            .WithAll<Sharpness, SharpeningProgress>();
    }

    public void Update(float deltaTime)
    {
        // Process all knives being sharpened
        _world.Query(in _sharpeningQuery, (ref Entity entity, ref Sharpness sharpness, ref SharpeningProgress progress) =>
        {
            // Use pure logic to calculate sharpening (testable without ECS)
            float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(
                progress.InitialLevel, sharpness.MaxLevel, progress.Duration, deltaTime);

            sharpness.Level = SharpeningLogic.ApplySharpeningProgress(
                sharpness.Level, sharpenAmount, sharpness.MaxLevel);

            progress.ElapsedTime += deltaTime;

            // Emit progress event for UI updates
            float progressPercent = SharpeningLogic.CalculateProgressPercent(
                progress.ElapsedTime, progress.Duration);
            _facade.EmitEvent(new SharpeningProgressEvent(entity.Id, progressPercent));

            // Check if sharpening is complete (pure logic)
            if (SharpeningLogic.IsComplete(progress.ElapsedTime, progress.Duration))
            {
                // Ensure sharpness is exactly at max level
                sharpness.Level = sharpness.MaxLevel;

                // Emit completion event
                _facade.EmitEvent(new KnifeSharpenedEvent(entity.Id, sharpness.Level));

                // Remove SharpeningProgress component (done sharpening)
                entity.Remove<SharpeningProgress>();
            }
        });
    }
}

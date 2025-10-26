using Arch.Core;
using CookingProject.Logic.Core.Components;
using CookingProject.Logic.Features.Sharpening.Commands;
using CookingProject.Logic.Features.Sharpening.Components;
using CookingProject.Logic.Features.Sharpening.Events;

namespace CookingProject.Logic;

/// <summary>
/// Sharpening feature command handlers for GameFacade.
/// </summary>
public partial class GameFacade
{
    private void HandleStartSharpening(StartSharpeningCommand command)
    {
        var entity = command.KnifeEntity;

        if (!_world.IsAlive(entity))
        {
            return; // Entity doesn't exist
        }

        if (!_world.Has<Sharpness>(entity))
        {
            return; // Entity doesn't have Sharpness component
        }

        // Don't start if already sharpening
        if (_world.Has<SharpeningProgress>(entity))
        {
            return;
        }

        // Capture current sharpness level before starting
        ref var sharpness = ref _world.Get<Sharpness>(entity);

        // Add SharpeningProgress component to begin sharpening
        _world.Add(entity, new SharpeningProgress
        {
            InitialLevel = sharpness.Level,
            ElapsedTime = 0f,
            Duration = command.Duration
        });

        EmitEvent(new SharpeningStartedEvent(entity.Id, command.Duration));
    }

    private void HandleCancelSharpening(CancelSharpeningCommand command)
    {
        var entity = command.KnifeEntity;

        if (!_world.IsAlive(entity))
        {
            return;
        }

        if (_world.Has<SharpeningProgress>(entity))
        {
            ref var progress = ref _world.Get<SharpeningProgress>(entity);
            float partialProgress = progress.ElapsedTime / progress.Duration;

            // Remove SharpeningProgress component (cancel sharpening)
            _world.Remove<SharpeningProgress>(entity);

            EmitEvent(new SharpeningCancelledEvent(entity.Id, partialProgress));
        }
    }

    /// <summary>
    /// Creates a test knife entity for demonstration purposes.
    /// In a real game, this would be called from a system or initialization code.
    /// </summary>
    /// <param name="toolType">The tool type (e.g., "Chef's Knife").</param>
    /// <returns>The created entity.</returns>
    public Entity CreateTestKnife(string toolType)
    {
        var entity = _world.Create(
            new Tool { ToolType = toolType },
            new Sharpness { Level = 0.5f, MaxLevel = 1.0f }
        );

        return entity;
    }
}

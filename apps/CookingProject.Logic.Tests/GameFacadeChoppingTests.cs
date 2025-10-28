using Arch.Core;
using CookingProject.Logic.Features.Chopping.Commands;
using CookingProject.Logic.Features.Chopping.Components;
using CookingProject.Logic.Features.Chopping.Events;
using CookingProject.Logic.Features.Sharpening.Components;
using CookingProject.Logic.Features.Sharpening.Events;
using Xunit;

namespace CookingProject.Logic.Tests;

/// <summary>
/// Integration tests for GameFacade chopping command processing and event flow.
/// Verifies that chopping commands are routed correctly and events are emitted.
/// </summary>
[Collection("Sequential")]
public class GameFacadeChoppingTests : IDisposable
{
    private readonly GameFacade _facade;

    public GameFacadeChoppingTests()
    {
        _facade = new GameFacade();
        _facade.Initialize();
    }

    public void Dispose()
    {
        _facade.Dispose();
    }

    // ========================================
    // StartChoppingCommand Integration Tests
    // ========================================

    [Fact]
    public void ProcessCommand_StartChopping_AddsChoppingProgressComponent()
    {
        // Arrange: Create knife and ingredient entities
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);

        // Act: Send StartChoppingCommand
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));

        // Assert: Ingredient has ChoppingProgress component
        Assert.True(_facade.World.Has<ChoppingProgress>(ingredient));
    }

    [Fact]
    public void ProcessCommand_StartChopping_EmitsChoppingStartedEvent()
    {
        // Arrange
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);

        // Act
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        var events = _facade.ConsumeEvents();

        // Assert: ChoppingStartedEvent emitted
        var startedEvent = events.OfType<ChoppingStartedEvent>().FirstOrDefault();
        Assert.NotNull(startedEvent);
        // Note: Don't assert entity IDs - not reliable outside Arch ECS queries
    }

    [Fact]
    public void ProcessCommand_StartChoppingInvalidEntity_DoesNotEmitEvent()
    {
        // Arrange: Invalid entities
        var invalidKnife = default(Entity);
        var invalidIngredient = default(Entity);

        // Act
        _facade.ProcessCommand(new StartChoppingCommand(invalidIngredient, invalidKnife));
        var events = _facade.ConsumeEvents();

        // Assert: No events emitted
        Assert.Empty(events);
    }

    [Fact]
    public void ProcessCommand_StartChoppingMissingComponents_DoesNotEmitEvent()
    {
        // Arrange: Entity without required components
        var knife = CreateKnife(sharpness: 1.0f);
        var entity = _facade.World.Create(); // No Ingredient or ChoppableItem

        // Act
        _facade.ProcessCommand(new StartChoppingCommand(entity, knife));
        var events = _facade.ConsumeEvents();

        // Assert: No events
        Assert.Empty(events);
    }

    // ========================================
    // CancelChoppingCommand Integration Tests
    // ========================================

    [Fact]
    public void ProcessCommand_CancelChopping_RemovesChoppingProgress()
    {
        // Arrange: Start chopping first
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));

        // Act: Cancel chopping
        _facade.ProcessCommand(new CancelChoppingCommand(ingredient));

        // Assert: ChoppingProgress removed
        Assert.False(_facade.World.Has<ChoppingProgress>(ingredient));
    }

    [Fact]
    public void ProcessCommand_CancelChopping_EmitsChoppingCancelledEvent()
    {
        // Arrange: Start chopping and advance progress
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        _facade.Update(0.5f); // 50% progress
        _facade.ConsumeEvents(); // Clear started/progress events

        // Act: Cancel chopping
        _facade.ProcessCommand(new CancelChoppingCommand(ingredient));
        var events = _facade.ConsumeEvents();

        // Assert: CancelledEvent emitted with partial progress
        var cancelledEvent = events.OfType<ChoppingCancelledEvent>().FirstOrDefault();
        Assert.NotNull(cancelledEvent);
        Assert.InRange(cancelledEvent.PartialProgress, 0.0f, 1.0f);
    }

    [Fact]
    public void ProcessCommand_CancelChoppingNotBeingChopped_DoesNotEmitEvent()
    {
        // Arrange: Ingredient not being chopped
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);

        // Act
        _facade.ProcessCommand(new CancelChoppingCommand(ingredient));
        var events = _facade.ConsumeEvents();

        // Assert: No events
        Assert.Empty(events);
    }

    // ========================================
    // Full Integration: Update Loop Tests
    // ========================================

    [Fact]
    public void UpdateLoop_ChoppingInProgress_EmitsProgressEvents()
    {
        // Arrange
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        _facade.ConsumeEvents(); // Clear started event

        // Act: Update 3 times
        _facade.Update(0.1f);
        var events1 = _facade.ConsumeEvents();
        _facade.Update(0.1f);
        var events2 = _facade.ConsumeEvents();
        _facade.Update(0.1f);
        var events3 = _facade.ConsumeEvents();

        // Assert: Each update emits a progress event
        Assert.Single(events1.OfType<ChoppingProgressEvent>());
        Assert.Single(events2.OfType<ChoppingProgressEvent>());
        Assert.Single(events3.OfType<ChoppingProgressEvent>());
    }

    [Fact]
    public void UpdateLoop_ChopCompletes_EmitsChoppedEvent()
    {
        // Arrange
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        _facade.ConsumeEvents(); // Clear started event

        // Act: Complete the chop (soft tomato = 0.8s base, sharp knife = 0.8s)
        _facade.Update(0.8f);
        var events = _facade.ConsumeEvents();

        // Assert: IngredientChoppedEvent emitted
        var choppedEvent = events.OfType<IngredientChoppedEvent>().FirstOrDefault();
        Assert.NotNull(choppedEvent);
        Assert.Equal(1, choppedEvent.CurrentChops);
        Assert.False(choppedEvent.FullyChopped); // Needs 3 total
    }

    [Fact]
    public void UpdateLoop_LastChopCompletes_EmitsFullyPreparedEvent()
    {
        // Arrange: Ingredient needs 1 chop total
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 1);
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        _facade.ConsumeEvents();

        // Act: Complete the chop
        _facade.Update(0.8f);
        var events = _facade.ConsumeEvents();

        // Assert: Both ChoppedEvent and FullyPreparedEvent emitted
        Assert.NotNull(events.OfType<IngredientChoppedEvent>().FirstOrDefault());
        Assert.NotNull(events.OfType<IngredientFullyPreparedEvent>().FirstOrDefault());
    }

    [Fact]
    public void UpdateLoop_ChopCompletes_DegradesKnife()
    {
        // Arrange
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        _facade.ConsumeEvents();

        // Act: Complete chop
        _facade.Update(0.8f);
        var events = _facade.ConsumeEvents();

        // Assert: Knife sharpness reduced
        ref var sharpness = ref _facade.World.Get<Sharpness>(knife);
        Assert.True(sharpness.Level < 1.0f); // Degraded from 1.0

        // Assert: KnifeDegradedEvent emitted
        Assert.NotNull(events.OfType<KnifeDegradedEvent>().FirstOrDefault());
    }

    [Fact]
    public void EndToEnd_StartChopUpdateComplete_FullWorkflow()
    {
        // Arrange: Create entities
        var knife = CreateKnife(sharpness: 1.0f);
        var ingredient = CreateIngredient(IngredientType.Tomato, IngredientHardness.Soft, requiredChops: 3);

        // Act & Assert: Start chopping
        _facade.ProcessCommand(new StartChoppingCommand(ingredient, knife));
        var startEvents = _facade.ConsumeEvents();
        Assert.Single(startEvents.OfType<ChoppingStartedEvent>());

        // Act & Assert: Progress through partial chop
        _facade.Update(0.4f); // 50% progress (0.4 / 0.8)
        var progressEvents = _facade.ConsumeEvents();
        var progressEvent = progressEvents.OfType<ChoppingProgressEvent>().First();
        Assert.InRange(progressEvent.Progress, 0.49f, 0.51f); // ~50%

        // Act & Assert: Complete chop
        _facade.Update(0.5f); // Finish chop (0.4 + 0.5 = 0.9s > 0.8s needed)
        var completeEvents = _facade.ConsumeEvents();
        var choppedEvent = completeEvents.OfType<IngredientChoppedEvent>().FirstOrDefault();
        Assert.NotNull(choppedEvent);
        Assert.Equal(1, choppedEvent.CurrentChops); // First chop completed
        Assert.NotNull(completeEvents.OfType<KnifeDegradedEvent>().FirstOrDefault());

        // Note: ChoppingProgress component removal is tested in ChoppingSystemTests
        // and ProcessCommand_CancelChopping_RemovesChoppingProgress test above
    }

    // ========================================
    // Helper Methods
    // ========================================

    private Entity CreateKnife(float sharpness)
    {
        return _facade.World.Create(
            new Sharpness { Level = sharpness, MaxLevel = 1.0f }
        );
    }

    private Entity CreateIngredient(IngredientType type, IngredientHardness hardness, int requiredChops)
    {
        var ingredient = hardness switch
        {
            IngredientHardness.Soft => Ingredient.CreateSoft(type),
            IngredientHardness.Medium => Ingredient.CreateMedium(type),
            IngredientHardness.Hard => Ingredient.CreateHard(type),
            _ => throw new ArgumentException("Invalid hardness")
        };

        return _facade.World.Create(
            ingredient,
            new ChoppableItem(requiredChops)
        );
    }
}

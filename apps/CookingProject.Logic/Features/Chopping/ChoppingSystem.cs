using Arch.Core;
using Arch.Core.Extensions;
using CookingProject.Logic.Core.Commands;
using CookingProject.Logic.Core.Events;
using CookingProject.Logic.Core.Systems;
using CookingProject.Logic.Features.Chopping.Commands;
using CookingProject.Logic.Features.Chopping.Components;
using CookingProject.Logic.Features.Chopping.Events;
using CookingProject.Logic.Features.Chopping.Logic;
using CookingProject.Logic.Features.Sharpening.Components;
using CookingProject.Logic.Features.Sharpening.Events;

namespace CookingProject.Logic.Features.Chopping;

/// <summary>
/// Handles ingredient chopping mechanics, progress tracking, and knife degradation.
/// Integrates with sharpening system through Sharpness component.
/// </summary>
public class ChoppingSystem : IGameSystem
{
    private readonly World _world;
    private readonly GameFacade _facade;
    private readonly QueryDescription _choppingQuery;

    public ChoppingSystem(World world, GameFacade facade)
    {
        _world = world;
        _facade = facade;

        // Query for entities actively being chopped
        _choppingQuery = new QueryDescription()
            .WithAll<Ingredient, ChoppableItem, ChoppingProgress>();
    }

    /// <summary>
    /// Update all chopping entities, tracking progress and completing chops.
    /// </summary>
    public void Update(float deltaTime)
    {
        // Collect completed chop entities (can't modify archetype during query)
        var completedChops = new List<Entity>();

        // Query all entities being chopped
        _world.Query(in _choppingQuery, (ref Entity entity, ref Ingredient ingredient, ref ChoppableItem choppable, ref ChoppingProgress progress) =>
        {
            // Advance elapsed time
            progress.ElapsedTime += deltaTime;

            // Calculate and emit progress event
            float progressPercent = ChoppingLogic.CalculateProgress(progress.ElapsedTime, progress.ChopDuration);
            _facade.EmitEvent(new ChoppingProgressEvent(entity.Id, progressPercent));

            // Check if chop is complete
            if (ChoppingLogic.ShouldCompleteChop(progress.ElapsedTime, progress.ChopDuration))
            {
                CompleteChop(ref choppable, ref ingredient, progress.KnifeEntity, entity.Id);
                completedChops.Add(entity); // Store copy for later removal
            }
        });

        // Remove ChoppingProgress from completed chops (after query)
        foreach (var entity in completedChops)
        {
            if (_world.IsAlive(entity) && _world.Has<ChoppingProgress>(entity))
            {
                _world.Remove<ChoppingProgress>(entity);
            }
        }
    }

    /// <summary>
    /// Complete a single chop: increment count, degrade knife, check if fully prepared.
    /// </summary>
    private void CompleteChop(ref ChoppableItem choppable, ref Ingredient ingredient, Entity knifeEntity, int ingredientEntityId)
    {
        // Increment chop count
        choppable.CurrentChops = ChoppingLogic.IncrementChops(choppable.CurrentChops, choppable.RequiredChops);

        // Degrade knife if it exists and has Sharpness component
        if (_world.IsAlive(knifeEntity) && _world.Has<Sharpness>(knifeEntity))
        {
            ref var sharpness = ref _world.Get<Sharpness>(knifeEntity);
            float oldSharpness = sharpness.Level;
            sharpness.Level = ChoppingLogic.ApplyDegradation(sharpness.Level, ingredient.DegradationAmount);

            // Emit degradation event
            _facade.EmitEvent(new KnifeDegradedEvent(knifeEntity.Id, sharpness.Level));
        }

        // Check if fully chopped
        choppable.IsFullyChopped = ChoppingLogic.IsFullyChopped(choppable.CurrentChops, choppable.RequiredChops);

        // Emit chop completion event
        _facade.EmitEvent(new IngredientChoppedEvent(
            ingredientEntityId,
            choppable.CurrentChops,
            choppable.IsFullyChopped
        ));

        // Emit fully prepared event if complete
        if (choppable.IsFullyChopped)
        {
            _facade.EmitEvent(new IngredientFullyPreparedEvent(ingredientEntityId));
        }

        // Note: ChoppingProgress is removed in Update() after this method returns
    }

    /// <summary>
    /// Process game commands related to chopping.
    /// </summary>
    public void ProcessCommand(IGameCommand command)
    {
        switch (command)
        {
            case StartChoppingCommand start:
                HandleStartChopping(start);
                break;

            case CancelChoppingCommand cancel:
                HandleCancelChopping(cancel);
                break;
        }
    }

    /// <summary>
    /// Start chopping an ingredient with a knife.
    /// </summary>
    private void HandleStartChopping(StartChoppingCommand cmd)
    {
        var ingredientEntity = cmd.IngredientEntity;
        var knifeEntity = cmd.KnifeEntity;

        // Validate ingredient entity
        if (!_world.IsAlive(ingredientEntity))
        {
            return; // Entity doesn't exist
        }

        if (!_world.Has<Ingredient>(ingredientEntity) || !_world.Has<ChoppableItem>(ingredientEntity))
        {
            return; // Missing required components
        }

        // Don't start chopping if already in progress
        if (_world.Has<ChoppingProgress>(ingredientEntity))
        {
            return; // Already being chopped
        }

        // Validate knife entity
        if (!_world.IsAlive(knifeEntity))
        {
            return; // Knife doesn't exist
        }

        if (!_world.Has<Sharpness>(knifeEntity))
        {
            return; // Not a valid knife (no sharpness component)
        }

        // Get ingredient and knife data
        ref var ingredient = ref _world.Get<Ingredient>(ingredientEntity);
        ref var choppable = ref _world.Get<ChoppableItem>(ingredientEntity);
        ref var sharpness = ref _world.Get<Sharpness>(knifeEntity);

        // Check if already fully chopped (optional - could allow re-chopping)
        if (choppable.IsFullyChopped)
        {
            return; // Already fully prepared
        }

        // Calculate chop duration based on knife sharpness
        float chopDuration = ChoppingLogic.CalculateChopTime(ingredient.BaseChopTime, sharpness.Level);

        // Add ChoppingProgress component
        _world.Add(ingredientEntity, new ChoppingProgress(knifeEntity, chopDuration));

        // Emit started event
        _facade.EmitEvent(new ChoppingStartedEvent(ingredientEntity.Id, knifeEntity.Id, chopDuration));
    }

    /// <summary>
    /// Cancel active chopping without completing the chop or degrading the knife.
    /// </summary>
    private void HandleCancelChopping(CancelChoppingCommand cmd)
    {
        var ingredientEntity = cmd.IngredientEntity;

        // Validate entity exists
        if (!_world.IsAlive(ingredientEntity))
        {
            return;
        }

        // Check if entity is being chopped
        if (!_world.Has<ChoppingProgress>(ingredientEntity))
        {
            return; // Not being chopped
        }

        // Get partial progress before removing
        ref var progress = ref _world.Get<ChoppingProgress>(ingredientEntity);
        float partialProgress = ChoppingLogic.CalculateProgress(progress.ElapsedTime, progress.ChopDuration);

        // Remove ChoppingProgress component
        _world.Remove<ChoppingProgress>(ingredientEntity);

        // Emit cancelled event
        _facade.EmitEvent(new ChoppingCancelledEvent(ingredientEntity.Id, partialProgress));
    }
}

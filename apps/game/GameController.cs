using Arch.Core;
using Godot;
using CookingProject.Logic;
using CookingProject.Logic.Commands;
using CookingProject.Logic.Events;

namespace CookingProject;

/// <summary>
/// Main game controller that bridges Godot and the ECS backend.
/// This script should be attached to an autoload singleton or the root game node.
/// </summary>
public partial class GameController : Node
{
    private GameFacade? _gameFacade;

    // Test ingredient entities (in a real game, these would be tracked properly)
    private Entity _testTomato;
    private Entity _testOnion;

    public override void _Ready()
    {
        GD.Print("Initializing Game Controller...");

        // Create and initialize the game facade
        _gameFacade = new GameFacade();
        _gameFacade.Initialize();

        // Create some test ingredients for demonstration
        _testTomato = _gameFacade.CreateTestIngredient("Tomato");
        _testOnion = _gameFacade.CreateTestIngredient("Onion");

        GD.Print($"Created test ingredients: Tomato={_testTomato.Id}, Onion={_testOnion.Id}");
        GD.Print("Game Controller initialized!");
    }

    public override void _Process(double delta)
    {
        if (_gameFacade == null)
        {
            return;
        }

        // Update ECS systems
        _gameFacade.Update((float)delta);

        // Process events from ECS and update UI
        var events = _gameFacade.ConsumeEvents();
        foreach (var evt in events)
        {
            HandleGameEvent(evt);
        }
    }

    /// <summary>
    /// Handles game events from the ECS backend.
    /// This is where you would update UI, play sounds, trigger animations, etc.
    /// </summary>
    private void HandleGameEvent(IGameEvent evt)
    {
        switch (evt)
        {
            case IngredientChoppedEvent chopEvent:
                GD.Print($"Ingredient chopped: {chopEvent.IngredientName} (Entity {chopEvent.EntityId})");
                // In a real game:
                // - Play chopping animation
                // - Play chopping sound
                // - Update UI to show ingredient is chopped
                break;

            case CookingProgressEvent progressEvent:
                // Update progress bar UI
                // GD.Print($"Cooking progress: {progressEvent.Progress:P0} at {progressEvent.Temperature:F1}°");
                break;

            case IngredientBurnedEvent burnEvent:
                GD.Print($"WARNING: {burnEvent.IngredientName} burned! (Entity {burnEvent.EntityId})");
                // In a real game:
                // - Play burning animation
                // - Play burning sound
                // - Show "BURNED!" popup
                // - Update ingredient visual to show burned state
                break;

            case RecipeCompletedEvent recipeEvent:
                GD.Print($"Recipe completed: {recipeEvent.RecipeName} - Score: {recipeEvent.Score}");
                // In a real game:
                // - Show success popup
                // - Update score UI
                // - Play success sound/animation
                // - Award points/achievements
                break;

            default:
                GD.PrintErr($"Unhandled event type: {evt.GetType().Name}");
                break;
        }
    }

    /// <summary>
    /// Example: Called when player clicks on an ingredient to chop it.
    /// This demonstrates sending a command from Godot to ECS.
    /// </summary>
    public void OnIngredientClicked(Entity ingredientEntity)
    {
        if (_gameFacade == null)
        {
            return;
        }

        GD.Print($"Player clicked ingredient {ingredientEntity.Id}");
        _gameFacade.ProcessCommand(new ChopIngredientCommand(ingredientEntity));
    }

    /// <summary>
    /// Example: Called when player starts cooking a recipe.
    /// </summary>
    public void OnStartCooking(Entity recipeEntity, float heatLevel = 0.8f)
    {
        if (_gameFacade == null)
        {
            return;
        }

        GD.Print($"Starting to cook recipe {recipeEntity.Id} at heat level {heatLevel:P0}");
        _gameFacade.ProcessCommand(new StartCookingCommand(recipeEntity, heatLevel));
    }

    /// <summary>
    /// Example: Called when player adjusts stove temperature.
    /// </summary>
    public void OnHeatAdjusted(Entity stoveEntity, float newHeatLevel)
    {
        if (_gameFacade == null)
        {
            return;
        }

        GD.Print($"Adjusting heat for entity {stoveEntity.Id} to {newHeatLevel:P0}");
        _gameFacade.ProcessCommand(new AdjustHeatCommand(stoveEntity, newHeatLevel));
    }

    /// <summary>
    /// Example input handling for testing.
    /// Press keys to test different commands.
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (_gameFacade == null)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Key1:
                    // Chop tomato
                    OnIngredientClicked(_testTomato);
                    break;

                case Key.Key2:
                    // Chop onion
                    OnIngredientClicked(_testOnion);
                    break;

                case Key.Key3:
                    // Start cooking tomato at medium heat
                    OnStartCooking(_testTomato, 0.5f);
                    break;

                case Key.Key4:
                    // Start cooking tomato at high heat (will burn!)
                    OnStartCooking(_testTomato, 1.0f);
                    break;

                case Key.Key5:
                    // Reduce heat
                    OnHeatAdjusted(_testTomato, 0.3f);
                    break;

                case Key.Key6:
                    // Increase heat
                    OnHeatAdjusted(_testTomato, 0.9f);
                    break;

                default:
                    break;
            }
        }
    }

    public override void _ExitTree()
    {
        // Clean up when game ends
        _gameFacade?.Dispose();
        GD.Print("Game Controller disposed");
    }
}

using Arch.Core;
using Godot;
using CookingProject.Logic;
using CookingProject.Logic.Core.Events;
using CookingProject.Logic.Features.Sharpening.Commands;
using CookingProject.Logic.Features.Sharpening.Events;

namespace CookingProject;

/// <summary>
/// Main game controller that bridges Godot and the ECS backend.
/// This script should be attached to an autoload singleton or the root game node.
/// </summary>
public partial class GameController : Node
{
    private GameFacade? _gameFacade;

    // Test knife entity (in a real game, this would be tracked properly)
    private Entity _testKnife;

    public override void _Ready()
    {
        GD.Print("Initializing Game Controller...");

        // Create and initialize the game facade
        _gameFacade = new GameFacade();
        _gameFacade.Initialize();

        // Create a test knife for demonstration
        _testKnife = _gameFacade.CreateTestKnife("Chef's Knife");

        GD.Print($"Created test knife: Chef's Knife (Entity {_testKnife.Id})");
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
            case SharpeningStartedEvent startEvent:
                GD.Print($"Sharpening started on knife (Entity {startEvent.EntityId}), duration: {startEvent.Duration}s");
                // In a real game:
                // - Play sharpening animation
                // - Play sharpening sound
                // - Show progress bar
                break;

            case SharpeningProgressEvent progressEvent:
                // Update progress bar UI
                GD.Print($"Sharpening progress: {progressEvent.Progress:P0}");
                break;

            case KnifeSharpenedEvent sharpenedEvent:
                GD.Print($"Knife sharpened! (Entity {sharpenedEvent.EntityId}), new sharpness: {sharpenedEvent.FinalSharpness:F2}");
                // In a real game:
                // - Play completion sound
                // - Show success animation
                // - Update knife visual to show sharpness
                break;

            case SharpeningCancelledEvent cancelEvent:
                GD.Print($"Sharpening cancelled (Entity {cancelEvent.EntityId}) at {cancelEvent.PartialProgress:P0}");
                // In a real game:
                // - Stop animation
                // - Hide progress bar
                // - Play cancel sound
                break;

            case KnifeDegradedEvent degradeEvent:
                GD.Print($"Knife degraded (Entity {degradeEvent.EntityId}), new sharpness: {degradeEvent.NewSharpness:F2}");
                // In a real game:
                // - Update sharpness indicator
                // - Play dulling sound if appropriate
                break;

            default:
                GD.PrintErr($"Unhandled event type: {evt.GetType().Name}");
                break;
        }
    }

    /// <summary>
    /// Example: Called when player clicks on whetstone to start sharpening.
    /// This demonstrates sending a command from Godot to ECS.
    /// </summary>
    public void OnStartSharpening(Entity knifeEntity, float duration = 5.0f)
    {
        if (_gameFacade == null)
        {
            return;
        }

        GD.Print($"Player started sharpening knife {knifeEntity.Id}");
        _gameFacade.ProcessCommand(new StartSharpeningCommand(knifeEntity, duration));
    }

    /// <summary>
    /// Example: Called when player cancels sharpening.
    /// </summary>
    public void OnCancelSharpening(Entity knifeEntity)
    {
        if (_gameFacade == null)
        {
            return;
        }

        GD.Print($"Player cancelled sharpening knife {knifeEntity.Id}");
        _gameFacade.ProcessCommand(new CancelSharpeningCommand(knifeEntity));
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
                    // Start sharpening with default duration (5 seconds)
                    OnStartSharpening(_testKnife);
                    break;

                case Key.Key2:
                    // Start sharpening with fast duration (3 seconds)
                    OnStartSharpening(_testKnife, 3.0f);
                    break;

                case Key.Key3:
                    // Cancel sharpening
                    OnCancelSharpening(_testKnife);
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

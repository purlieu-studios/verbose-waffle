using System;
using Arch.Core;
using Godot;
using CookingProject.Logic;
using CookingProject.Logic.Core.Events;
using CookingProject.Logic.Features.Movement.Commands;
using CookingProject.Logic.Features.Movement.Components;
using CookingProject.Logic.Features.Sharpening.Commands;
using CookingProject.Logic.Features.Sharpening.Events;
using LogicVector2 = CookingProject.Logic.Core.Math.Vector2;

namespace CookingProject;

/// <summary>
/// Main game controller that bridges Godot and the ECS backend.
/// This script should be attached to an autoload singleton or the root game node.
/// </summary>
public partial class GameController : Node
{
    private GameFacade? _gameFacade;

    // Test entities (in a real game, this would be tracked properly)
    private Entity _testKnife;
    private Entity _testMovingEntity;

    // Visual representation (Godot node) for the moving entity
    private Sprite2D? _testSprite;

    /// <summary>
    /// Test helper: Check if the test moving entity exists.
    /// Returns true if the entity is alive in the ECS world.
    /// </summary>
    public bool TestEntityExists()
    {
        return _gameFacade != null && _gameFacade.World.IsAlive(_testMovingEntity);
    }

    /// <summary>
    /// Test helper: Get the current position of the test moving entity.
    /// Returns Vector2.ZERO if entity doesn't exist.
    /// </summary>
    public Vector2 GetTestEntityPosition()
    {
        if (_gameFacade == null || !_gameFacade.World.IsAlive(_testMovingEntity))
        {
            return Vector2.Zero;
        }

        if (_gameFacade.World.Has<Position>(_testMovingEntity))
        {
            ref var position = ref _gameFacade.World.Get<Position>(_testMovingEntity);
            return new Vector2(position.Value.X, position.Value.Y);
        }

        return Vector2.Zero;
    }

    /// <summary>
    /// Test helper: Send input command to the test entity.
    /// </summary>
    public void TestSendMoveCommand(Vector2 direction)
    {
        if (_gameFacade != null)
        {
            var logicVelocity = new LogicVector2(direction.X, direction.Y);
            _gameFacade.ProcessCommand(new SetVelocityCommand(_testMovingEntity, logicVelocity));
        }
    }

    private int _frameCounter;
    private ulong _totalFrames;  // Total frame count since start
    private const int DebugWriteInterval = 60; // Write debug files every 60 frames (~1 second at 60 FPS)

    public override void _Ready()
    {
        DebugLogger.Log("Initializing Game Controller...");

#if DEBUG
        // Install crash dump handler
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DebugLogger.Log("Crash dump handler installed");
#endif

        // Create and initialize the game facade
        _gameFacade = new GameFacade();
        _gameFacade.Initialize();

#if DEBUG
        // Enable debug systems
        _gameFacade.EnableDebug();
        DebugLogger.Log("Debug systems enabled");
#endif

        // Create a test knife for demonstration
        _testKnife = _gameFacade.CreateTestKnife("Chef's Knife");
        DebugLogger.Log($"Created test knife: Chef's Knife (Entity {_testKnife.Id})");

        // Create a test moving entity
        _testMovingEntity = _gameFacade.CreateTestMovingEntity("Moving Object", new LogicVector2(100, 100));
        DebugLogger.Log($"Created test moving entity at (100, 100) (Entity {_testMovingEntity.Id})");

        // Get the visual sprite node (sibling node, not child)
        // This is optional - only test scenes will have it
        _testSprite = GetNodeOrNull<Sprite2D>("../TestSprite");
        if (_testSprite != null)
        {
            _testSprite.Modulate = Colors.Red;
            DebugLogger.Log("Found TestSprite node, will sync with ECS position");
        }

        DebugLogger.Log("Game Controller initialized!");
    }

    public override void _Process(double delta)
    {
        if (_gameFacade == null)
        {
            return;
        }

        // Update ECS systems
        _gameFacade.Update((float)delta);

        // Sync ECS positions to Godot visuals
        SyncECSToGodot();

        // Process events from ECS and update UI
        var events = _gameFacade.ConsumeEvents();
        foreach (var evt in events)
        {
            HandleGameEvent(evt);
        }

#if DEBUG
        // Periodically write debug data to JSON files
        _totalFrames++;
        _frameCounter++;
        if (_frameCounter >= DebugWriteInterval)
        {
            WriteDebugFiles(_totalFrames);
            _frameCounter = 0;
        }
#endif
    }

    /// <summary>
    /// Syncs ECS component data to Godot visual nodes.
    /// In a real game, this would use a proper entity-to-node mapping system.
    /// </summary>
    private void SyncECSToGodot()
    {
        if (_gameFacade == null || _testSprite == null)
        {
            return;
        }

        // Get the Position component from our test moving entity
        var world = _gameFacade.World;
        if (world.Has<Position>(_testMovingEntity))
        {
            ref var position = ref world.Get<Position>(_testMovingEntity);
            // Convert our engine-agnostic LogicVector2 to Godot's Vector2
            _testSprite.Position = new Vector2(position.Value.X, position.Value.Y);
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
                DebugLogger.Log($"Sharpening started on knife (Entity {startEvent.EntityId}), duration: {startEvent.Duration}s");
                // In a real game:
                // - Play sharpening animation
                // - Play sharpening sound
                // - Show progress bar
                break;

            case SharpeningProgressEvent progressEvent:
                // Update progress bar UI
                DebugLogger.Log($"Sharpening progress: {progressEvent.Progress:P0}");
                break;

            case KnifeSharpenedEvent sharpenedEvent:
                DebugLogger.Log($"Knife sharpened! (Entity {sharpenedEvent.EntityId}), new sharpness: {sharpenedEvent.FinalSharpness:F2}");
                // In a real game:
                // - Play completion sound
                // - Show success animation
                // - Update knife visual to show sharpness
                break;

            case SharpeningCancelledEvent cancelEvent:
                DebugLogger.Log($"Sharpening cancelled (Entity {cancelEvent.EntityId}) at {cancelEvent.PartialProgress:P0}");
                // In a real game:
                // - Stop animation
                // - Hide progress bar
                // - Play cancel sound
                break;

            case KnifeDegradedEvent degradeEvent:
                DebugLogger.Log($"Knife degraded (Entity {degradeEvent.EntityId}), new sharpness: {degradeEvent.NewSharpness:F2}");
                // In a real game:
                // - Update sharpness indicator
                // - Play dulling sound if appropriate
                break;

            default:
                DebugLogger.LogError($"Unhandled event type: {evt.GetType().Name}");
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

        DebugLogger.Log($"Player started sharpening knife {knifeEntity.Id}");
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

        DebugLogger.Log($"Player cancelled sharpening knife {knifeEntity.Id}");
        _gameFacade.ProcessCommand(new CancelSharpeningCommand(knifeEntity));
    }

    /// <summary>
    /// Example input handling for testing.
    /// Press keys to test different commands.
    /// Debug: F12 = Manual debug snapshot
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

                // Movement controls with arrow keys
                case Key.Right:
                    _gameFacade.ProcessCommand(new SetVelocityCommand(_testMovingEntity, new LogicVector2(100, 0)));
                    DebugLogger.Log("Moving right (velocity: 100, 0)");
                    break;

                case Key.Left:
                    _gameFacade.ProcessCommand(new SetVelocityCommand(_testMovingEntity, new LogicVector2(-100, 0)));
                    DebugLogger.Log("Moving left (velocity: -100, 0)");
                    break;

                case Key.Up:
                    _gameFacade.ProcessCommand(new SetVelocityCommand(_testMovingEntity, new LogicVector2(0, -100)));
                    DebugLogger.Log("Moving up (velocity: 0, -100)");
                    break;

                case Key.Down:
                    _gameFacade.ProcessCommand(new SetVelocityCommand(_testMovingEntity, new LogicVector2(0, 100)));
                    DebugLogger.Log("Moving down (velocity: 0, 100)");
                    break;

                case Key.Space:
                    // Stop movement
                    _gameFacade.ProcessCommand(new SetVelocityCommand(_testMovingEntity, LogicVector2.Zero));
                    DebugLogger.Log("Stopped movement (velocity: 0, 0)");
                    break;

#if DEBUG
                case Key.F12:
                    // Manual debug snapshot (saves to debug_snapshots/ with timestamp)
                    DebugLogger.Log($"[F12] Manual debug snapshot at frame {_totalFrames}");
                    WriteDebugFiles(_totalFrames, isSnapshot: true);
                    break;
#endif

                default:
                    break;
            }
        }
    }

    public override void _ExitTree()
    {
#if DEBUG
        // Write final debug snapshot before exiting
        WriteDebugFiles(_totalFrames);
#endif

        // Clean up when game ends
        _gameFacade?.Dispose();
        DebugLogger.Log("Game Controller disposed");
    }

#if DEBUG
    /// <summary>
    /// Unhandled exception handler - dumps debug state on crash.
    /// </summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            DebugLogger.LogError($"[CRASH] Unhandled exception detected:");
            DebugLogger.LogError(e.ExceptionObject.ToString() ?? "Unknown exception");
            DebugLogger.LogError($"[CRASH] Dumping debug state at frame {_totalFrames}...");

            // Write crash dump with snapshot
            WriteDebugFiles(_totalFrames, isSnapshot: true);

            DebugLogger.LogError("[CRASH] Debug state dumped to debug_snapshots/");
        }
        catch (Exception dumpEx)
        {
            DebugLogger.LogError($"[CRASH] Failed to dump debug state: {dumpEx.Message}");
        }
    }

    /// <summary>
    /// Write debug data to JSON files for Claude to read.
    /// </summary>
    private void WriteDebugFiles(ulong frameNumber, bool isSnapshot = false)
    {
        if (_gameFacade?.IsDebugEnabled != true)
        {
            return;
        }

        // Write performance data
        if (_gameFacade.Profiler != null)
        {
            var perfJson = _gameFacade.Profiler.ExportJson(frameCount: 60);
            DebugLogger.WriteJsonFile("performance.json", perfJson);
            if (isSnapshot)
            {
                DebugLogger.WriteSnapshotFile("performance", perfJson);
            }
        }

        // Write event/command log
        if (_gameFacade.EventLogger != null)
        {
            var eventsJson = _gameFacade.EventLogger.ExportJson(maxEntries: 100);
            DebugLogger.WriteJsonFile("events.json", eventsJson);
            if (isSnapshot)
            {
                DebugLogger.WriteSnapshotFile("events", eventsJson);
            }
        }

        // Write ECS state snapshot (lightweight, entity lifecycle)
        if (_gameFacade.Inspector != null)
        {
            var stateJson = _gameFacade.Inspector.ExportWorldSnapshot();
            DebugLogger.WriteJsonFile("ecs_state.json", stateJson);
            if (isSnapshot)
            {
                DebugLogger.WriteSnapshotFile("ecs_state", stateJson);
            }
        }

        // Write archetype information (component types per archetype)
        if (_gameFacade.ArchetypeInspector != null)
        {
            var archetypesJson = _gameFacade.ArchetypeInspector.ExportSnapshot(frameNumber);
            DebugLogger.WriteJsonFile("archetypes.json", archetypesJson);
            if (isSnapshot)
            {
                DebugLogger.WriteSnapshotFile("archetypes", archetypesJson);
            }
        }

        // Write full entity component data (allocates memory, shows actual values)
        if (_gameFacade.ArchetypeInspector != null)
        {
            var entitiesJson = _gameFacade.ArchetypeInspector.ExportEntityDetails(frameNumber);
            DebugLogger.WriteJsonFile("entities.json", entitiesJson);
            if (isSnapshot)
            {
                DebugLogger.WriteSnapshotFile("entities", entitiesJson);
            }
        }
    }
#endif
}

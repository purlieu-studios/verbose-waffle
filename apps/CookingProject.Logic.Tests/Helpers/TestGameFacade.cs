using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Tests.Helpers;

/// <summary>
/// Test double for GameFacade that captures events for assertion.
/// </summary>
public class TestGameFacade : GameFacade
{
    private readonly List<IGameEvent> _capturedEvents = new();

    /// <summary>
    /// Captures events for test inspection.
    /// Overrides the internal EmitEvent method (accessible via InternalsVisibleTo).
    /// </summary>
    internal override void EmitEvent(IGameEvent gameEvent)
    {
        _capturedEvents.Add(gameEvent);
        // Call base internal method to maintain proper event queue behavior
        base.EmitEvent(gameEvent);
    }

    /// <summary>
    /// Gets all captured events of a specific type.
    /// </summary>
    public IEnumerable<T> GetAllEvents<T>() where T : IGameEvent
    {
        return _capturedEvents.OfType<T>();
    }

    /// <summary>
    /// Gets the last event of a specific type, or null if none exist.
    /// </summary>
    public T? GetLastEvent<T>() where T : IGameEvent
    {
        return _capturedEvents.OfType<T>().LastOrDefault();
    }

    /// <summary>
    /// Gets all captured events.
    /// </summary>
    public IEnumerable<IGameEvent> GetAllEvents()
    {
        return _capturedEvents;
    }

    /// <summary>
    /// Clears all captured events.
    /// </summary>
    public void ClearEvents()
    {
        _capturedEvents.Clear();
    }

    /// <summary>
    /// Gets the count of captured events.
    /// </summary>
    public int EventCount => _capturedEvents.Count;
}

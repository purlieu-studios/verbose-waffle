using CookingProject.Logic.Debug;

namespace CookingProject.Logic;

/// <summary>
/// GameFacade partial class for debug functionality.
/// Only compiled in DEBUG builds.
/// </summary>
public partial class GameFacade
{
#if DEBUG
    private SystemProfiler? _profiler;
    private EventCommandLogger? _eventLogger;
    private ECSStateInspector? _inspector;
    private ArchetypeInspector? _archetypeInspector;
    private bool _debugEnabled;

    /// <summary>
    /// Enable debug systems.
    /// Must be called after Initialize().
    /// </summary>
    public void EnableDebug()
    {
        if (_world == null)
        {
            throw new InvalidOperationException("Cannot enable debug before Initialize() is called");
        }

        _profiler = new SystemProfiler();
        _eventLogger = new EventCommandLogger();
        _inspector = new ECSStateInspector(_world);
        _archetypeInspector = new ArchetypeInspector(_world);
        _debugEnabled = true;
    }

    /// <summary>
    /// Disable debug systems and clear data.
    /// </summary>
    public void DisableDebug()
    {
        _profiler = null;
        _eventLogger = null;
        _inspector = null;
        _archetypeInspector = null;
        _debugEnabled = false;
    }

    /// <summary>
    /// Get the system profiler (null if debug not enabled).
    /// </summary>
    public SystemProfiler? Profiler => _profiler;

    /// <summary>
    /// Get the event/command logger (null if debug not enabled).
    /// </summary>
    public EventCommandLogger? EventLogger => _eventLogger;

    /// <summary>
    /// Get the ECS state inspector (null if debug not enabled).
    /// </summary>
    public ECSStateInspector? Inspector => _inspector;

    /// <summary>
    /// Get the archetype inspector for viewing component data (null if debug not enabled).
    /// </summary>
    public ArchetypeInspector? ArchetypeInspector => _archetypeInspector;

    /// <summary>
    /// Is debug enabled?
    /// </summary>
    public bool IsDebugEnabled => _debugEnabled;
#else
    /// <summary>
    /// Debug is not available in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be instance method to match DEBUG build API
    public void EnableDebug()
#pragma warning restore CA1822 // Mark members as static
    {
        // No-op in release builds
    }

    /// <summary>
    /// Debug is not available in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be instance method to match DEBUG build API
    public void DisableDebug()
#pragma warning restore CA1822 // Mark members as static
    {
        // No-op in release builds
    }

    /// <summary>
    /// Always false in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be property to match DEBUG build API
    public bool IsDebugEnabled => false;
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Always null in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be property to match DEBUG build API
    public SystemProfiler? Profiler => null;
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Always null in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be property to match DEBUG build API
    public EventCommandLogger? EventLogger => null;
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Always null in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be property to match DEBUG build API
    public ECSStateInspector? Inspector => null;
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Always null in release builds.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static - Must be property to match DEBUG build API
    public ArchetypeInspector? ArchetypeInspector => null;
#pragma warning restore CA1822 // Mark members as static
#endif
}

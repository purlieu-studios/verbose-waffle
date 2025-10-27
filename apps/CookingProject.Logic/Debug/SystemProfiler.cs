using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CookingProject.Logic.Debug;

/// <summary>
/// Profiles system performance and tracks frame timing.
/// Only compiled in DEBUG builds.
/// </summary>
public class SystemProfiler
{
    private readonly Stopwatch _frameTimer = new();
    private readonly Stopwatch _systemTimer = new();
    private readonly List<FrameProfile> _frameHistory = new();
    private FrameProfile? _currentFrame;
    private SystemProfile? _currentSystem;
    private int _frameNumber;
    private const int MaxFrameHistory = 300; // Keep last 5 seconds at 60 FPS

    /// <summary>
    /// Start timing a new frame.
    /// </summary>
    public void StartFrame()
    {
        _frameNumber++;
        _frameTimer.Restart();
        _currentFrame = new FrameProfile
        {
            FrameNumber = _frameNumber,
            Timestamp = DateTime.Now,
            Systems = new List<SystemProfile>()
        };
    }

    /// <summary>
    /// Start timing a system.
    /// </summary>
    public void StartSystem(string systemName, int entityCount = 0)
    {
        _systemTimer.Restart();
        _currentSystem = new SystemProfile
        {
            Name = systemName,
            EntityCount = entityCount
        };
    }

    /// <summary>
    /// End timing the current system.
    /// </summary>
    public void EndSystem()
    {
        if (_currentSystem != null && _currentFrame != null)
        {
            _currentSystem.ExecutionTimeMs = _systemTimer.Elapsed.TotalMilliseconds;
            _currentFrame.Systems.Add(_currentSystem);
            _currentSystem = null;
        }
    }

    /// <summary>
    /// End timing the current frame.
    /// </summary>
    public void EndFrame()
    {
        if (_currentFrame != null)
        {
            _currentFrame.TotalFrameTimeMs = _frameTimer.Elapsed.TotalMilliseconds;
            _currentFrame.FPS = _currentFrame.TotalFrameTimeMs > 0
                ? 1000.0 / _currentFrame.TotalFrameTimeMs
                : 0;

            // Add to history
            _frameHistory.Add(_currentFrame);

            // Keep only recent frames
            if (_frameHistory.Count > MaxFrameHistory)
            {
                _frameHistory.RemoveAt(0);
            }

            _currentFrame = null;
        }
    }

    /// <summary>
    /// Get performance summary for the last N frames.
    /// </summary>
    public PerformanceSummary GetSummary(int frameCount = 60)
    {
        var recentFrames = _frameHistory.TakeLast(frameCount).ToList();

        if (recentFrames.Count == 0)
        {
            return new PerformanceSummary();
        }

        var avgFps = recentFrames.Average(f => f.FPS);
        var avgFrameTime = recentFrames.Average(f => f.TotalFrameTimeMs);

        // Group by system name and calculate averages
        var systemStats = recentFrames
            .SelectMany(f => f.Systems)
            .GroupBy(s => s.Name)
            .Select(g => new SystemStats
            {
                Name = g.Key,
                AvgExecutionTimeMs = g.Average(s => s.ExecutionTimeMs),
                AvgEntityCount = g.Average(s => s.EntityCount),
                CallCount = g.Count()
            })
            .OrderByDescending(s => s.AvgExecutionTimeMs)
            .ToList();

        return new PerformanceSummary
        {
            FrameCount = recentFrames.Count,
            AverageFPS = avgFps,
            AverageFrameTimeMs = avgFrameTime,
            SystemStats = systemStats
        };
    }

    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Export profiling data as JSON.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public string ExportJson(int frameCount = 60)
    {
        var summary = GetSummary(frameCount);
        return JsonSerializer.Serialize(summary, s_jsonOptions);
    }

    /// <summary>
    /// Get the most recent frame profile.
    /// </summary>
    public FrameProfile? GetLastFrame()
    {
        return _frameHistory.LastOrDefault();
    }
}

/// <summary>
/// Profile data for a single frame.
/// </summary>
public class FrameProfile
{
    public int FrameNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public double TotalFrameTimeMs { get; set; }
    public double FPS { get; set; }
    public List<SystemProfile> Systems { get; init; } = new();
}

/// <summary>
/// Profile data for a single system execution.
/// </summary>
public class SystemProfile
{
    public string Name { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public int EntityCount { get; set; }
}

/// <summary>
/// Performance summary over multiple frames.
/// </summary>
public class PerformanceSummary
{
    public int FrameCount { get; set; }
    public double AverageFPS { get; set; }
    public double AverageFrameTimeMs { get; set; }
    public List<SystemStats> SystemStats { get; init; } = new();
}

/// <summary>
/// Aggregated statistics for a system.
/// </summary>
public class SystemStats
{
    public string Name { get; set; } = string.Empty;
    public double AvgExecutionTimeMs { get; set; }
    public double AvgEntityCount { get; set; }
    public int CallCount { get; set; }
}

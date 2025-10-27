using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CookingProject.Logic.Core.Commands;
using CookingProject.Logic.Core.Events;

namespace CookingProject.Logic.Debug;

/// <summary>
/// Logs all commands and events for debugging and replay.
/// Only compiled in DEBUG builds.
/// </summary>
public class EventCommandLogger
{
    private readonly ConcurrentQueue<LogEntry> _log = new();
    private const int MaxLogSize = 1000; // Keep last N entries
    private int _sequenceNumber;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = false };
    private static readonly JsonSerializerOptions s_prettyJsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Log a command being processed.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public void LogCommand(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var entry = new LogEntry
        {
            SequenceNumber = Interlocked.Increment(ref _sequenceNumber),
            Timestamp = DateTime.Now,
            Type = "Command",
            Name = command.GetType().Name,
            Data = JsonSerializer.Serialize(command, s_jsonOptions)
        };

        AddEntry(entry);
    }

    /// <summary>
    /// Log an event being emitted.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public void LogEvent(IGameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);

        var entry = new LogEntry
        {
            SequenceNumber = Interlocked.Increment(ref _sequenceNumber),
            Timestamp = DateTime.Now,
            Type = "Event",
            Name = gameEvent.GetType().Name,
            Data = JsonSerializer.Serialize(gameEvent, s_jsonOptions)
        };

        AddEntry(entry);
    }

    /// <summary>
    /// Associate events with the command that triggered them.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public void AssociateCausality(IGameCommand command, List<IGameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(events);

        if (events.Count == 0)
        {
            return;
        }

        var causalityEntry = new LogEntry
        {
            SequenceNumber = Interlocked.Increment(ref _sequenceNumber),
            Timestamp = DateTime.Now,
            Type = "Causality",
            Name = $"{command.GetType().Name} → {events.Count} events",
            Data = JsonSerializer.Serialize(new
            {
                Command = command.GetType().Name,
                Events = events.Select(e => e.GetType().Name).ToList()
            }, s_jsonOptions)
        };

        AddEntry(causalityEntry);
    }

    private void AddEntry(LogEntry entry)
    {
        _log.Enqueue(entry);

        // Trim to max size
        while (_log.Count > MaxLogSize)
        {
            _log.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Get all log entries.
    /// </summary>
    public List<LogEntry> GetAllEntries()
    {
        return _log.ToList();
    }

    /// <summary>
    /// Get recent log entries.
    /// </summary>
    public List<LogEntry> GetRecentEntries(int count = 50)
    {
        return _log.TakeLast(count).ToList();
    }

    /// <summary>
    /// Filter entries by type (Command, Event, Causality).
    /// </summary>
    public List<LogEntry> FilterByType(string type)
    {
        return _log.Where(e => e.Type == type).ToList();
    }

    /// <summary>
    /// Export log as JSON.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public string ExportJson(int? maxEntries = null)
    {
        var entries = maxEntries.HasValue
            ? _log.TakeLast(maxEntries.Value).ToList()
            : _log.ToList();

        return JsonSerializer.Serialize(entries, s_prettyJsonOptions);
    }

    /// <summary>
    /// Clear the log.
    /// </summary>
    public void Clear()
    {
        while (_log.TryDequeue(out _)) { }
        _sequenceNumber = 0;
    }

    /// <summary>
    /// Get statistics about the log.
    /// </summary>
    public LogStats GetStats()
    {
        var entries = _log.ToList();
        return new LogStats
        {
            TotalEntries = entries.Count,
            CommandCount = entries.Count(e => e.Type == "Command"),
            EventCount = entries.Count(e => e.Type == "Event"),
            CausalityCount = entries.Count(e => e.Type == "Causality")
        };
    }
}

/// <summary>
/// A single log entry for a command or event.
/// </summary>
public class LogEntry
{
    public int SequenceNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty; // Command, Event, Causality
    public string Name { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Statistics about the event/command log.
/// </summary>
public class LogStats
{
    public int TotalEntries { get; set; }
    public int CommandCount { get; set; }
    public int EventCount { get; set; }
    public int CausalityCount { get; set; }
}

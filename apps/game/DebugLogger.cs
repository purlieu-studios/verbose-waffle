using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace CookingProject;

/// <summary>
/// Captures all game output and writes it to a file that external tools (like Claude MCP) can read.
/// Also forwards to Godot's normal output.
/// </summary>
public partial class DebugLogger : Node
{
    private const string LogFileName = "game_output.log";
    private static DebugLogger? _instance;
    private StreamWriter? _logWriter;
    private readonly Queue<string> _logBuffer = new();
    private readonly object _lock = new();
    private string _logPath = "";

    public static DebugLogger Instance => _instance ?? throw new InvalidOperationException("DebugLogger not initialized");

    public override void _Ready()
    {
        if (_instance != null)
        {
            QueueFree();
            return;
        }

        _instance = this;

        // Log to project root so it's easily accessible
        _logPath = ProjectSettings.GlobalizePath($"res://{LogFileName}");

        try
        {
            // Open log file in append mode
            _logWriter = new StreamWriter(_logPath, append: false) // Start fresh each run
            {
                AutoFlush = true // Flush immediately so external tools can read
            };

            Log("=== Game Output Log Started ===");
            Log($"Log file: {_logPath}");
            Log($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log("================================\n");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to open log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Log a message. This replaces GD.Print() for loggable output.
    /// </summary>
    public static void Log(string message)
    {
        if (_instance == null)
        {
            // Fallback if logger not initialized yet
            GD.Print(message);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var formattedMessage = $"[{timestamp}] {message}";

        // Print to Godot console
        GD.Print(message);

        // Write to log file
        lock (_instance._lock)
        {
            try
            {
                _instance._logWriter?.WriteLine(formattedMessage);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to write to log: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Log an error message.
    /// </summary>
    public static void LogError(string message)
    {
        if (_instance == null)
        {
            GD.PrintErr(message);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var formattedMessage = $"[{timestamp}] ERROR: {message}";

        GD.PrintErr(message);

        lock (_instance._lock)
        {
            try
            {
                _instance._logWriter?.WriteLine(formattedMessage);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to write error to log: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Log a warning message.
    /// </summary>
    public static void LogWarning(string message)
    {
        if (_instance == null)
        {
            GD.PushWarning(message);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var formattedMessage = $"[{timestamp}] WARNING: {message}";

        GD.PushWarning(message);

        lock (_instance._lock)
        {
            try
            {
                _instance._logWriter?.WriteLine(formattedMessage);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to write warning to log: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Log a formatted message (like string.Format).
    /// </summary>
    public static void LogFormat(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    /// <summary>
    /// Log a separator line for readability.
    /// </summary>
    public static void LogSeparator()
    {
        Log("─────────────────────────────────────────────────────");
    }

    /// <summary>
    /// Log a section header.
    /// </summary>
    public static void LogSection(string sectionName)
    {
        Log("");
        Log($"═══ {sectionName} ═══");
        Log("");
    }

    public override void _ExitTree()
    {
        lock (_lock)
        {
            if (_logWriter != null)
            {
                Log("\n=== Game Output Log Ended ===");
                _logWriter.Flush();
                _logWriter.Dispose();
                _logWriter = null;
            }
        }

        _instance = null;
    }

    /// <summary>
    /// Get the current log file path (useful for external tools).
    /// </summary>
    public static string GetLogPath()
    {
        return _instance?._logPath ?? "";
    }
}

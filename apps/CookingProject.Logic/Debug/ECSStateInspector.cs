using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Arch.Core;

namespace CookingProject.Logic.Debug;

/// <summary>
/// Inspects and exports ECS world state for debugging.
/// Only compiled in DEBUG builds.
/// </summary>
public class ECSStateInspector
{
    private readonly World _world;
    private readonly List<(int EntityId, DateTime Timestamp)> _entityCreated = new();
    private readonly List<(int EntityId, DateTime Timestamp)> _entityDestroyed = new();
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public ECSStateInspector(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Get count of total entities in the world.
    /// Note: Arch requires a query, so we count all entities with any archetype.
    /// </summary>
    public int GetEntityCount()
    {
        int count = 0;
        // Count all entities by querying with no restrictions
        _world.Query(new QueryDescription(), (ref Entity _) => count++);
        return count;
    }

    /// <summary>
    /// Export summary of world state as JSON.
    /// Since Arch doesn't provide dynamic component introspection,
    /// we export statistics and entity IDs only.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public string ExportWorldSnapshot()
    {
        var entityIds = new List<int>();
        _world.Query(new QueryDescription(), (ref Entity entity) => entityIds.Add(entity.Id));

        var snapshot = new
        {
            Timestamp = DateTime.Now,
            EntityCount = entityIds.Count,
            EntityIds = entityIds,
            CreatedLog = _entityCreated.TakeLast(100).ToList(),
            DestroyedLog = _entityDestroyed.TakeLast(100).ToList()
        };

        return JsonSerializer.Serialize(snapshot, s_jsonOptions);
    }

    /// <summary>
    /// Log when an entity is created.
    /// </summary>
    public void LogEntityCreated(Entity entity)
    {
        _entityCreated.Add((entity.Id, DateTime.Now));
    }

    /// <summary>
    /// Log when an entity is destroyed.
    /// </summary>
    public void LogEntityDestroyed(Entity entity)
    {
        _entityDestroyed.Add((entity.Id, DateTime.Now));
    }

    /// <summary>
    /// Get entity lifecycle statistics.
    /// </summary>
    public EntityStats EntityStats => new()
    {
        Created = _entityCreated.Count,
        Destroyed = _entityDestroyed.Count,
        Active = GetEntityCount()
    };
}

/// <summary>
/// Statistics about entity lifecycle.
/// </summary>
public class EntityStats
{
    public int Created { get; init; }
    public int Destroyed { get; init; }
    public int Active { get; init; }
}

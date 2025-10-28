using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Arch.Core;
using Arch.Core.Extensions;

namespace CookingProject.Logic.Debug;

/// <summary>
/// Inspects archetypes and provides detailed archetype/chunk information.
/// Requires Arch.Core.Extensions namespace for extension methods.
/// </summary>
public class ArchetypeInspector
{
    private readonly World _world;

    // JSON options for component value serialization (fields only, skip readonly properties)
    private static readonly JsonSerializerOptions s_componentJsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,  // Required to serialize public fields in structs
        IgnoreReadOnlyProperties = true,  // Ignore readonly properties like Magnitude
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never  // Include all fields
    };

    // JSON options for snapshot exports (include properties for anonymous objects)
    private static readonly JsonSerializerOptions s_snapshotJsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public ArchetypeInspector(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Gets a summary of all archetypes in the world.
    /// </summary>
    public List<ArchetypeInfo> GetArchetypes()
    {
        var archetypes = new List<ArchetypeInfo>();

        // Iterate all archetypes using Arch's foreach support
        foreach (ref var archetype in _world)
        {
            var info = new ArchetypeInfo
            {
                EntityCount = archetype.EntityCount,
                ChunkCount = archetype.ChunkCount,
                ComponentTypes = GetComponentTypeNames(archetype.Signature)
            };

            archetypes.Add(info);
        }

        return archetypes;
    }

    /// <summary>
    /// Gets all entities in the world with their component type information.
    /// </summary>
    public List<EntityInfo> GetAllEntities()
    {
        var entities = new List<EntityInfo>();

        // Iterate archetypes to get entity component information
        // This avoids calling extension methods inside Query lambdas (causes AccessViolationException)
        foreach (ref var archetype in _world)
        {
            var componentTypes = GetComponentTypeNames(archetype.Signature);

            // Get all entities in this archetype
            for (int chunkIndex = 0; chunkIndex < archetype.ChunkCount; chunkIndex++)
            {
                ref var chunk = ref archetype.GetChunk(chunkIndex);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = chunk.Entity(i);
                    entities.Add(new EntityInfo
                    {
                        EntityId = entity.Id,
                        ComponentTypes = componentTypes
                    });
                }
            }
        }

        return entities;
    }

    /// <summary>
    /// Gets detailed component data for a specific entity.
    /// Allocates memory to retrieve actual component values.
    /// </summary>
    public EntityDetailInfo? GetEntityDetails(Entity entity)
    {
        if (!_world.IsAlive(entity))
        {
            return null;
        }

        var componentData = new List<ComponentData>();

        // Use ComponentRegistry to iterate through all known component types
        foreach (var componentType in ComponentRegistry.GetAllComponentTypes())
        {
            try
            {
                // Find the Has<T> method
                var hasMethods = typeof(World).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Has" &&
                                m.IsGenericMethodDefinition &&
                                m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(Entity) &&
                                m.ReturnType == typeof(bool))
                    .ToArray();

                if (hasMethods.Length == 0)
                {
                    continue;
                }

                var hasMethod = hasMethods[0];
                var genericHasMethod = hasMethod.MakeGenericMethod(componentType);
                var hasComponent = (bool)(genericHasMethod.Invoke(_world, new object[] { entity }) ?? false);

                if (!hasComponent)
                {
                    continue;
                }

                // Entity has this component, now get its value
                var getMethods = typeof(World).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Get" &&
                                m.IsGenericMethodDefinition &&
                                m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(Entity))
                    .ToArray();

                if (getMethods.Length == 0)
                {
                    continue;
                }

                var getMethod = getMethods[0]; // Use first match
                var genericGetMethod = getMethod.MakeGenericMethod(componentType);

                // Invoke returns boxed value even if method returns ref T
                var componentValue = genericGetMethod.Invoke(_world, new object[] { entity });

                if (componentValue != null)
                {
                    // Serialize using the actual component type, not the boxed object type
                    var json = JsonSerializer.Serialize(componentValue, componentType, s_componentJsonOptions);
                    componentData.Add(new ComponentData
                    {
                        TypeName = componentType.Name,
                        FullTypeName = componentType.FullName ?? componentType.Name,
                        Value = json
                    });
                }
            }
            catch (Exception ex) when (ex is TargetInvocationException or InvalidOperationException or ArgumentException)
            {
                // If we can't get the component, include error info
                componentData.Add(new ComponentData
                {
                    TypeName = componentType.Name,
                    FullTypeName = componentType.FullName ?? componentType.Name,
                    Value = $"{{\"error\": \"{ex.Message.Replace("\"", "\\\"", StringComparison.Ordinal)}\"}}"
                });
            }
        }

        return new EntityDetailInfo
        {
            EntityId = entity.Id,
            Components = componentData
        };
    }

    /// <summary>
    /// Exports a comprehensive snapshot of all archetypes and entities.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public string ExportSnapshot(ulong frameNumber = 0)
    {
        var snapshot = new
        {
            FrameNumber = frameNumber,
            Timestamp = DateTime.Now,
            Archetypes = GetArchetypes(),
            TotalEntities = GetAllEntities().Count
        };

        return JsonSerializer.Serialize(snapshot, s_snapshotJsonOptions);
    }

    /// <summary>
    /// Exports detailed information about all entities including component values.
    /// WARNING: Allocates memory for each entity's components.
    /// </summary>
    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "Static field used")]
    public string ExportEntityDetails(ulong frameNumber = 0)
    {
        var entityDetails = new List<EntityDetailInfo>();

        // Collect entities first, then get details outside the archetype loop
        var entitiesToInspect = new List<Entity>();
        foreach (ref var archetype in _world)
        {
            for (int chunkIndex = 0; chunkIndex < archetype.ChunkCount; chunkIndex++)
            {
                ref var chunk = ref archetype.GetChunk(chunkIndex);
                for (int i = 0; i < chunk.Count; i++)
                {
                    entitiesToInspect.Add(chunk.Entity(i));
                }
            }
        }

        // Now get details for each entity (uses extension methods safely)
        foreach (var entity in entitiesToInspect)
        {
            var details = GetEntityDetails(entity);
            if (details != null)
            {
                entityDetails.Add(details);
            }
        }

        var snapshot = new
        {
            FrameNumber = frameNumber,
            Timestamp = DateTime.Now,
            EntityCount = entityDetails.Count,
            Entities = entityDetails
        };

        return JsonSerializer.Serialize(snapshot, s_snapshotJsonOptions);
    }

    private static List<string> GetComponentTypeNames(Signature signature)
    {
        ComponentType[] types = signature;
        return GetComponentTypeNames(types);
    }

    private static List<string> GetComponentTypeNames(ComponentType[] types)
    {
        var names = new List<string>();
        foreach (var type in types)
        {
            names.Add(type.Type.Name);
        }
        return names;
    }
}

/// <summary>
/// Information about an archetype.
/// </summary>
public class ArchetypeInfo
{
    public int EntityCount { get; init; }
    public int ChunkCount { get; init; }
    public List<string> ComponentTypes { get; init; } = new();
}

/// <summary>
/// Basic information about an entity.
/// </summary>
public class EntityInfo
{
    public int EntityId { get; init; }
    public List<string> ComponentTypes { get; init; } = new();
}

/// <summary>
/// Detailed information about an entity including component values.
/// </summary>
public class EntityDetailInfo
{
    public int EntityId { get; init; }
    public List<ComponentData> Components { get; init; } = new();
}

/// <summary>
/// Component type and serialized value.
/// </summary>
public class ComponentData
{
    public required string TypeName { get; init; }
    public required string FullTypeName { get; init; }
    public required string Value { get; init; }
}

using Arch.Core;
using CookingProject.Logic.Debug;
using CookingProject.Logic.Features.Movement.Components;
using CookingProject.Logic.Features.Sharpening.Components;
using CookingProject.Logic.Features.Cooking.Components;
using Xunit;
using System.Text.Json;

namespace CookingProject.Logic.Tests.Debug;

/// <summary>
/// Tests for ArchetypeInspector to verify component viewing works.
/// </summary>
public class ArchetypeInspectorTests
{
    [Fact]
    public void CanGetArchetypeInformation()
    {
        // Arrange
        var world = World.Create();
        var inspector = new ArchetypeInspector(world);

        // Create entities with different archetypes
        world.Create(new Position { Value = new Logic.Core.Math.Vector2(10, 20) });
        world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(5, 5) },
            new Velocity { Value = new Logic.Core.Math.Vector2(1, 1) }
        );
        world.Create(new Sharpness { Level = 0.8f, MaxLevel = 1.0f });

        // Act
        var archetypes = inspector.GetArchetypes();

        // Assert
        Assert.NotEmpty(archetypes);
        Assert.True(archetypes.Count >= 3, $"Expected at least 3 archetypes, got {archetypes.Count}");

        // Verify archetype contains component types
        var positionArchetype = archetypes.FirstOrDefault(a => a.ComponentTypes.Contains("Position") && a.ComponentTypes.Count == 1);
        Assert.NotNull(positionArchetype);
        Assert.True(positionArchetype.EntityCount >= 1);

        world.Dispose();
    }

    [Fact]
    public void CanGetAllEntitiesWithComponentTypes()
    {
        // Arrange
        var world = World.Create();
        var inspector = new ArchetypeInspector(world);

        var entity1 = world.Create(new Position { Value = new Logic.Core.Math.Vector2(100, 200) });
        var entity2 = world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(50, 50) },
            new Velocity { Value = new Logic.Core.Math.Vector2(10, 10) }
        );

        // Act
        var entities = inspector.GetAllEntities();

        // Assert
        Assert.Equal(2, entities.Count);

        var e1 = entities.FirstOrDefault(e => e.EntityId == entity1.Id);
        Assert.NotNull(e1);
        Assert.Single(e1.ComponentTypes);
        Assert.Contains("Position", e1.ComponentTypes);

        var e2 = entities.FirstOrDefault(e => e.EntityId == entity2.Id);
        Assert.NotNull(e2);
        Assert.Equal(2, e2.ComponentTypes.Count);
        Assert.Contains("Position", e2.ComponentTypes);
        Assert.Contains("Velocity", e2.ComponentTypes);

        world.Dispose();
    }

    [Fact]
    public void CanGetEntityDetailsWithComponentValues()
    {
        // Arrange
        var world = World.Create();
        var inspector = new ArchetypeInspector(world);

        var entity = world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(123.45f, 678.90f) },
            new Velocity { Value = new Logic.Core.Math.Vector2(5.5f, 10.2f) }
        );

        // Act
        var details = inspector.GetEntityDetails(entity);

        // Assert
        Assert.NotNull(details);
        Assert.Equal(entity.Id, details.EntityId);
        Assert.Equal(2, details.Components.Count);

        // Find Position component
        var positionComponent = details.Components.FirstOrDefault(c => c.TypeName == "Position");
        Assert.NotNull(positionComponent);
        Assert.Contains("123.45", positionComponent.Value, StringComparison.Ordinal);
        Assert.Contains("678.9", positionComponent.Value, StringComparison.Ordinal);  // JSON removes trailing zeros

        // Find Velocity component
        var velocityComponent = details.Components.FirstOrDefault(c => c.TypeName == "Velocity");
        Assert.NotNull(velocityComponent);
        Assert.Contains("5.5", velocityComponent.Value, StringComparison.Ordinal);
        Assert.Contains("10.2", velocityComponent.Value, StringComparison.Ordinal);

        world.Dispose();
    }

    [Fact]
    public void CanExportSnapshotAsJson()
    {
        // Arrange
        var world = World.Create();
        var inspector = new ArchetypeInspector(world);

        world.Create(new Position { Value = new Logic.Core.Math.Vector2(10, 20) });
        world.Create(new Sharpness { Level = 0.8f, MaxLevel = 1.0f });

        // Act
        var json = inspector.ExportSnapshot();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);

        // Verify JSON is valid and contains expected data
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("Timestamp", out _));
        Assert.True(doc.RootElement.TryGetProperty("Archetypes", out var archetypesElement));
        Assert.True(doc.RootElement.TryGetProperty("TotalEntities", out var totalEntitiesElement));
        Assert.Equal(2, totalEntitiesElement.GetInt32());

        world.Dispose();
    }

    [Fact]
    public void CanExportEntityDetailsAsJson()
    {
        // Arrange
        var world = World.Create();
        var inspector = new ArchetypeInspector(world);

        world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(100, 200) },
            new Velocity { Value = new Logic.Core.Math.Vector2(5, 10) }
        );

        // Act
        var json = inspector.ExportEntityDetails();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);

        // Verify JSON contains actual component values
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("Entities", out var entitiesElement));
        Assert.Equal(1, entitiesElement.GetArrayLength());

        var firstEntity = entitiesElement[0];
        Assert.True(firstEntity.TryGetProperty("Components", out var componentsElement));
        Assert.Equal(2, componentsElement.GetArrayLength());

        // Verify component data is present
        var component0 = componentsElement[0];
        Assert.True(component0.TryGetProperty("TypeName", out _));
        Assert.True(component0.TryGetProperty("Value", out _));

        world.Dispose();
    }

    [Fact]
    public void WorksWithComplexComponents()
    {
        // Arrange
        var world = World.Create();
        var inspector = new ArchetypeInspector(world);

        // Create entity with cooking components
        var entity = world.Create(
            new HeatSource { CurrentHeat = 0.66f, HasCookingEntity = false },
            new CookingProgress
            {
                Doneness = 0.5f,
                OptimalHeatMin = 0.4f,
                OptimalHeatMax = 0.8f,
                CookTimeSeconds = 300f,
                IsOnHeat = true
            }
        );

        // Act
        var details = inspector.GetEntityDetails(entity);

        // Assert
        Assert.NotNull(details);
        Assert.Equal(2, details.Components.Count);

        // Verify HeatSource
        var heatSource = details.Components.FirstOrDefault(c => c.TypeName == "HeatSource");
        Assert.NotNull(heatSource);
        Assert.Contains("0.66", heatSource.Value, StringComparison.Ordinal);

        // Verify CookingProgress
        var cookingProgress = details.Components.FirstOrDefault(c => c.TypeName == "CookingProgress");
        Assert.NotNull(cookingProgress);
        Assert.Contains("0.5", cookingProgress.Value, StringComparison.Ordinal);
        Assert.Contains("300", cookingProgress.Value, StringComparison.Ordinal);

        world.Dispose();
    }
}

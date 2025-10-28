using Arch.Core;
using Arch.Core.Extensions;  // Try adding Extensions namespace
using CookingProject.Logic.Features.Movement.Components;
using CookingProject.Logic.Features.Sharpening.Components;
using CookingProject.Logic.Features.Cooking.Components;
using Xunit;

namespace CookingProject.Logic.Tests.Debug;

/// <summary>
/// Experimental tests to explore Arch's introspection APIs.
/// Used to verify what APIs are available in version 2.1.0.
/// </summary>
public class ArchIntrospectionTests
{
    [Fact]
    public void CanIterateArchetypes()
    {
        // Arrange: Create world with multiple archetypes
        var world = World.Create();

        // Create entities with different component combinations
        var entity1 = world.Create(new Position { Value = new Logic.Core.Math.Vector2(10, 20) });
        var entity2 = world.Create(new Position { Value = new Logic.Core.Math.Vector2(5, 5) }, new Velocity { Value = new Logic.Core.Math.Vector2(1, 1) });
        var entity3 = world.Create(new Sharpness { Level = 0.8f, MaxLevel = 1.0f });

        // Act: Try to iterate archetypes
        int archetypeCount = 0;
        foreach (ref var archetype in world)
        {
            archetypeCount++;
            // TODO: Explore what properties/methods archetype has in next test
        }

        // Assert: Should have at least 3 archetypes (one per entity combination)
        Assert.True(archetypeCount >= 3, $"Expected at least 3 archetypes, got {archetypeCount}");

        world.Dispose();
    }

    [Fact]
    public void CanUseHasAndGetForKnownComponents()
    {
        // Arrange
        var world = World.Create();
        var entity = world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(10, 20) },
            new Velocity { Value = new Logic.Core.Math.Vector2(1, 1) }
        );

        // Act & Assert: Verify Has<T> works
        Assert.True(world.Has<Position>(entity));
        Assert.True(world.Has<Velocity>(entity));
        Assert.False(world.Has<Sharpness>(entity));

        // Act & Assert: Verify Get<T> works
        ref var position = ref world.Get<Position>(entity);
        Assert.Equal(10, position.Value.X);
        Assert.Equal(20, position.Value.Y);

        world.Dispose();
    }

    [Fact]
    public void EntityGetComponentTypesWorks()
    {
        // Arrange
        var world = World.Create();
        var entity = world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(10, 20) },
            new Velocity { Value = new Logic.Core.Math.Vector2(1, 1) }
        );

        // Act: Call GetComponentTypes with extension method
        var signature = entity.GetComponentTypes();
        // Signature has implicit conversion to ComponentType[]
        ComponentType[] componentTypes = signature;

        // Assert
        Assert.NotNull(componentTypes);
        Assert.Equal(2, componentTypes.Length);

        // Check if component types can be identified
        bool hasPosition = false;
        bool hasVelocity = false;

        foreach (var componentType in componentTypes)
        {
            // ComponentType should have some way to identify the type
            // Let's see what we can do with it
            var typeString = componentType.ToString();

            if (typeString.Contains("Position", StringComparison.Ordinal))
            {
                hasPosition = true;
            }
            if (typeString.Contains("Velocity", StringComparison.Ordinal))
            {
                hasVelocity = true;
            }
        }

        Assert.True(hasPosition, "Should have Position component");
        Assert.True(hasVelocity, "Should have Velocity component");

        world.Dispose();
    }

    [Fact]
    public void EntityGetAllComponentsWorks()
    {
        // Arrange
        var world = World.Create();
        var entity = world.Create(
            new Position { Value = new Logic.Core.Math.Vector2(100, 200) },
            new Velocity { Value = new Logic.Core.Math.Vector2(5, 10) }
        );

        // Act: GetAllComponents allocates and returns actual component instances!
        var components = entity.GetAllComponents();

        // Assert
        Assert.NotNull(components);
        Assert.Equal(2, components.Length);

        // Find Position component
        Position? position = null;
        Velocity? velocity = null;

        foreach (var component in components)
        {
            if (component is Position pos)
            {
                position = pos;
            }
            else if (component is Velocity vel)
            {
                velocity = vel;
            }
        }

        Assert.NotNull(position);
        Assert.NotNull(velocity);
        Assert.Equal(100, position.Value.Value.X);
        Assert.Equal(200, position.Value.Value.Y);
        Assert.Equal(5, velocity.Value.Value.X);
        Assert.Equal(10, velocity.Value.Value.Y);

        world.Dispose();
    }

    [Fact]
    public void EntityGetArchetypeWorks()
    {
        // Arrange
        var world = World.Create();
        var entity = world.Create(new Position { Value = new Logic.Core.Math.Vector2(10, 20) });

        // Act: Get the archetype
        var archetype = entity.GetArchetype();

        // Assert: Archetype should have properties we can inspect
        Assert.NotNull(archetype);
        // archetype.Signature should contain Position type
        // archetype.EntityCount should be >= 1

        world.Dispose();
    }
}

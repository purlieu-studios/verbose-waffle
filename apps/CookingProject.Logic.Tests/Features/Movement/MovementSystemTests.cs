using Arch.Core;
using CookingProject.Logic.Core.Math;
using CookingProject.Logic.Features.Movement;
using CookingProject.Logic.Features.Movement.Components;
using FluentAssertions;

namespace CookingProject.Logic.Tests.Features.Movement;

public class MovementSystemTests : IDisposable
{
    private readonly World _world;
    private readonly MovementSystem _system;

    public MovementSystemTests()
    {
        _world = World.Create();
        _system = new MovementSystem(_world);
    }

    public void Dispose()
    {
        _system.Dispose();
        _world.Dispose();
    }

    [Fact]
    public void Update_EntityWithPositionAndVelocity_UpdatesPosition()
    {
        var entity = _world.Create(
            new Position(10f, 10f),
            new Velocity(5f, 0f)
        );

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(15f);
        position.Value.Y.Should().Be(10f);
    }

    [Fact]
    public void Update_MultipleEntities_UpdatesAll()
    {
        var entity1 = _world.Create(
            new Position(0f, 0f),
            new Velocity(10f, 0f)
        );
        var entity2 = _world.Create(
            new Position(0f, 0f),
            new Velocity(0f, 20f)
        );

        _system.Update(1f);

        ref var pos1 = ref _world.Get<Position>(entity1);
        ref var pos2 = ref _world.Get<Position>(entity2);

        pos1.Value.X.Should().Be(10f);
        pos1.Value.Y.Should().Be(0f);
        pos2.Value.X.Should().Be(0f);
        pos2.Value.Y.Should().Be(20f);
    }

    [Fact]
    public void Update_EntityWithoutVelocity_DoesNotUpdate()
    {
        var entity = _world.Create(new Position(10f, 10f));

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(10f);
        position.Value.Y.Should().Be(10f);
    }

    [Fact]
    public void Update_EntityWithoutPosition_DoesNotCrash()
    {
        _world.Create(new Velocity(5f, 5f));

        var act = () => _system.Update(1f);

        act.Should().NotThrow();
    }

    [Fact]
    public void Update_ZeroVelocity_DoesNotMoveEntity()
    {
        var entity = _world.Create(
            new Position(10f, 10f),
            new Velocity(0f, 0f)
        );

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(10f);
        position.Value.Y.Should().Be(10f);
    }

    [Fact]
    public void Update_ZeroDeltaTime_DoesNotMoveEntity()
    {
        var entity = _world.Create(
            new Position(10f, 10f),
            new Velocity(100f, 100f)
        );

        _system.Update(0f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(10f);
        position.Value.Y.Should().Be(10f);
    }

    [Fact]
    public void Update_NegativeVelocity_MovesBackward()
    {
        var entity = _world.Create(
            new Position(10f, 10f),
            new Velocity(-5f, -5f)
        );

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(5f);
        position.Value.Y.Should().Be(5f);
    }

    [Fact]
    public void Update_SmallDeltaTime_AppliesCorrectMovement()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(100f, 0f)
        );

        _system.Update(0.5f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(50f);
    }

    [Fact]
    public void Update_MultipleFrames_AccumulatesMovement()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(10f, 0f)
        );

        _system.Update(1f);
        _system.Update(1f);
        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(30f);
    }

    [Fact]
    public void Update_DiagonalMovement_Works()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(3f, 4f)
        );

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(3f);
        position.Value.Y.Should().Be(4f);
    }

    // Integration scenarios
    [Fact]
    public void Scenario_PlayerMovingRight_At100PixelsPerSecond()
    {
        var player = _world.Create(
            new Position(100f, 200f),
            new Velocity(100f, 0f)
        );

        _system.Update(0.016f); // ~60 FPS

        ref var position = ref _world.Get<Position>(player);
        position.Value.X.Should().BeApproximately(101.6f, 0.01f);
        position.Value.Y.Should().Be(200f);
    }

    [Fact]
    public void Scenario_ObjectMovingInCircle_VelocityChangesEachFrame()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(10f, 0f)
        );

        // Frame 1: Move right
        _system.Update(1f);
        ref var velocity = ref _world.Get<Velocity>(entity);
        velocity.Value = new Vector2(0f, 10f);

        // Frame 2: Move down
        _system.Update(1f);
        velocity.Value = new Vector2(-10f, 0f);

        // Frame 3: Move left
        _system.Update(1f);
        velocity.Value = new Vector2(0f, -10f);

        // Frame 4: Move up
        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(0f);
        position.Value.Y.Should().Be(0f);
    }

    [Fact]
    public void Scenario_StoppingMovement_SetVelocityToZero()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(100f, 0f)
        );

        _system.Update(1f);
        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(100f);

        // Stop moving
        ref var velocity = ref _world.Get<Velocity>(entity);
        velocity.Value = Vector2.Zero;

        _system.Update(1f);
        position.Value.X.Should().Be(100f); // No further movement
    }

    [Fact]
    public void Scenario_HighSpeedObject_MovesLargeDistance()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(1000f, 0f)
        );

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().Be(1000f);
    }

    [Fact]
    public void Scenario_SlowObject_MovesTinyDistance()
    {
        var entity = _world.Create(
            new Position(0f, 0f),
            new Velocity(0.1f, 0f)
        );

        _system.Update(1f);

        ref var position = ref _world.Get<Position>(entity);
        position.Value.X.Should().BeApproximately(0.1f, 0.001f);
    }
}

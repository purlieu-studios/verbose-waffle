using CookingProject.Logic.Core.Math;

namespace CookingProject.Logic.Features.Movement.Components;

/// <summary>
/// Velocity component for moving entities.
/// Units are in pixels per second.
/// </summary>
public struct Velocity
{
    public Vector2 Value;

    public Velocity(Vector2 value)
    {
        Value = value;
    }

    public Velocity(float x, float y)
    {
        Value = new Vector2(x, y);
    }
}

using CookingProject.Logic.Core.Math;

namespace CookingProject.Logic.Features.Movement.Components;

/// <summary>
/// Position component for entities in 2D space.
/// </summary>
public struct Position
{
    public Vector2 Value;

    public Position(Vector2 value)
    {
        Value = value;
    }

    public Position(float x, float y)
    {
        Value = new Vector2(x, y);
    }
}

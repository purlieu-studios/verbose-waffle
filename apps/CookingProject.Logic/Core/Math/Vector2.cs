namespace CookingProject.Logic.Core.Math;

/// <summary>
/// Engine-agnostic 2D vector for position, velocity, and direction.
/// Completely independent of Godot or any game engine.
/// </summary>
public struct Vector2
{
    public float X;
    public float Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    // Common vector constants
    public static readonly Vector2 Zero = new(0, 0);
    public static readonly Vector2 One = new(1, 1);
    public static readonly Vector2 Up = new(0, -1);
    public static readonly Vector2 Down = new(0, 1);
    public static readonly Vector2 Left = new(-1, 0);
    public static readonly Vector2 Right = new(1, 0);

    // Vector operations
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, float scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector2 operator *(float scalar, Vector2 v) => new(v.X * scalar, v.Y * scalar);
    public static Vector2 operator /(Vector2 v, float scalar) => new(v.X / scalar, v.Y / scalar);
    public static Vector2 operator -(Vector2 v) => new(-v.X, -v.Y);

    // Magnitude and normalization
    public readonly float Magnitude => MathF.Sqrt(X * X + Y * Y);
    public readonly float MagnitudeSquared => X * X + Y * Y;

    public readonly Vector2 Normalized()
    {
        float mag = Magnitude;
        return mag > 0 ? this / mag : Zero;
    }

    // Distance and dot product
    public static float Distance(Vector2 a, Vector2 b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

    // Lerp for smooth movement
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        t = System.Math.Clamp(t, 0f, 1f);
        return new Vector2(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t
        );
    }

    // Friendly method names for operators (CA2225)
    public static Vector2 Add(Vector2 a, Vector2 b) => a + b;
    public static Vector2 Subtract(Vector2 a, Vector2 b) => a - b;
    public static Vector2 Multiply(Vector2 v, float scalar) => v * scalar;
    public static Vector2 Divide(Vector2 v, float scalar) => v / scalar;
    public static Vector2 Negate(Vector2 v) => -v;

    public override readonly string ToString() => $"({X:F2}, {Y:F2})";
}

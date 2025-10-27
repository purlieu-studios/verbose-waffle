using CookingProject.Logic.Core.Math;
using FluentAssertions;

namespace CookingProject.Logic.Tests.Core.Math;

public class Vector2Tests
{
    [Fact]
    public void Constructor_WithXY_SetsValues()
    {
        var vec = new Vector2(3.5f, 4.2f);

        vec.X.Should().Be(3.5f);
        vec.Y.Should().Be(4.2f);
    }

    [Fact]
    public void Zero_ReturnsZeroVector()
    {
        Vector2.Zero.X.Should().Be(0f);
        Vector2.Zero.Y.Should().Be(0f);
    }

    [Fact]
    public void One_ReturnsOneVector()
    {
        Vector2.One.X.Should().Be(1f);
        Vector2.One.Y.Should().Be(1f);
    }

    [Fact]
    public void Up_ReturnsCorrectDirection()
    {
        Vector2.Up.X.Should().Be(0f);
        Vector2.Up.Y.Should().Be(-1f);
    }

    [Fact]
    public void Down_ReturnsCorrectDirection()
    {
        Vector2.Down.X.Should().Be(0f);
        Vector2.Down.Y.Should().Be(1f);
    }

    [Fact]
    public void Left_ReturnsCorrectDirection()
    {
        Vector2.Left.X.Should().Be(-1f);
        Vector2.Left.Y.Should().Be(0f);
    }

    [Fact]
    public void Right_ReturnsCorrectDirection()
    {
        Vector2.Right.X.Should().Be(1f);
        Vector2.Right.Y.Should().Be(0f);
    }

    [Fact]
    public void Addition_AddsVectors()
    {
        var a = new Vector2(3f, 4f);
        var b = new Vector2(1f, 2f);

        var result = a + b;

        result.X.Should().Be(4f);
        result.Y.Should().Be(6f);
    }

    [Fact]
    public void Add_MethodWorks()
    {
        var a = new Vector2(3f, 4f);
        var b = new Vector2(1f, 2f);

        var result = Vector2.Add(a, b);

        result.X.Should().Be(4f);
        result.Y.Should().Be(6f);
    }

    [Fact]
    public void Subtraction_SubtractsVectors()
    {
        var a = new Vector2(5f, 7f);
        var b = new Vector2(2f, 3f);

        var result = a - b;

        result.X.Should().Be(3f);
        result.Y.Should().Be(4f);
    }

    [Fact]
    public void Subtract_MethodWorks()
    {
        var a = new Vector2(5f, 7f);
        var b = new Vector2(2f, 3f);

        var result = Vector2.Subtract(a, b);

        result.X.Should().Be(3f);
        result.Y.Should().Be(4f);
    }

    [Fact]
    public void Multiplication_ScalesVector()
    {
        var vec = new Vector2(3f, 4f);

        var result = vec * 2f;

        result.X.Should().Be(6f);
        result.Y.Should().Be(8f);
    }

    [Fact]
    public void Multiplication_ScalarFirst_ScalesVector()
    {
        var vec = new Vector2(3f, 4f);

        var result = 2f * vec;

        result.X.Should().Be(6f);
        result.Y.Should().Be(8f);
    }

    [Fact]
    public void Multiply_MethodWorks()
    {
        var vec = new Vector2(3f, 4f);

        var result = Vector2.Multiply(vec, 2f);

        result.X.Should().Be(6f);
        result.Y.Should().Be(8f);
    }

    [Fact]
    public void Division_DividesVector()
    {
        var vec = new Vector2(6f, 8f);

        var result = vec / 2f;

        result.X.Should().Be(3f);
        result.Y.Should().Be(4f);
    }

    [Fact]
    public void Divide_MethodWorks()
    {
        var vec = new Vector2(6f, 8f);

        var result = Vector2.Divide(vec, 2f);

        result.X.Should().Be(3f);
        result.Y.Should().Be(4f);
    }

    [Fact]
    public void Negation_NegatesVector()
    {
        var vec = new Vector2(3f, -4f);

        var result = -vec;

        result.X.Should().Be(-3f);
        result.Y.Should().Be(4f);
    }

    [Fact]
    public void Negate_MethodWorks()
    {
        var vec = new Vector2(3f, -4f);

        var result = Vector2.Negate(vec);

        result.X.Should().Be(-3f);
        result.Y.Should().Be(4f);
    }

    [Fact]
    public void Magnitude_CalculatesLength()
    {
        var vec = new Vector2(3f, 4f);

        vec.Magnitude.Should().Be(5f);
    }

    [Fact]
    public void Magnitude_ZeroVector_ReturnsZero()
    {
        Vector2.Zero.Magnitude.Should().Be(0f);
    }

    [Fact]
    public void MagnitudeSquared_CalculatesLengthSquared()
    {
        var vec = new Vector2(3f, 4f);

        vec.MagnitudeSquared.Should().Be(25f);
    }

    [Fact]
    public void Normalized_ReturnsUnitVector()
    {
        var vec = new Vector2(3f, 4f);

        var result = vec.Normalized();

        result.Magnitude.Should().BeApproximately(1f, 0.001f);
        result.X.Should().BeApproximately(0.6f, 0.001f);
        result.Y.Should().BeApproximately(0.8f, 0.001f);
    }

    [Fact]
    public void Normalized_ZeroVector_ReturnsZero()
    {
        var result = Vector2.Zero.Normalized();

        result.X.Should().Be(0f);
        result.Y.Should().Be(0f);
    }

    [Fact]
    public void Distance_CalculatesDistanceBetweenVectors()
    {
        var a = new Vector2(0f, 0f);
        var b = new Vector2(3f, 4f);

        var distance = Vector2.Distance(a, b);

        distance.Should().Be(5f);
    }

    [Fact]
    public void Distance_SamePoint_ReturnsZero()
    {
        var a = new Vector2(5f, 5f);
        var b = new Vector2(5f, 5f);

        var distance = Vector2.Distance(a, b);

        distance.Should().Be(0f);
    }

    [Fact]
    public void Dot_CalculatesDotProduct()
    {
        var a = new Vector2(2f, 3f);
        var b = new Vector2(4f, 5f);

        var dot = Vector2.Dot(a, b);

        dot.Should().Be(23f); // (2*4) + (3*5) = 8 + 15 = 23
    }

    [Fact]
    public void Dot_PerpendicularVectors_ReturnsZero()
    {
        var a = new Vector2(1f, 0f);
        var b = new Vector2(0f, 1f);

        var dot = Vector2.Dot(a, b);

        dot.Should().Be(0f);
    }

    [Fact]
    public void Lerp_AtZero_ReturnsFirst()
    {
        var a = new Vector2(0f, 0f);
        var b = new Vector2(10f, 10f);

        var result = Vector2.Lerp(a, b, 0f);

        result.X.Should().Be(0f);
        result.Y.Should().Be(0f);
    }

    [Fact]
    public void Lerp_AtOne_ReturnsSecond()
    {
        var a = new Vector2(0f, 0f);
        var b = new Vector2(10f, 10f);

        var result = Vector2.Lerp(a, b, 1f);

        result.X.Should().Be(10f);
        result.Y.Should().Be(10f);
    }

    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var a = new Vector2(0f, 0f);
        var b = new Vector2(10f, 10f);

        var result = Vector2.Lerp(a, b, 0.5f);

        result.X.Should().Be(5f);
        result.Y.Should().Be(5f);
    }

    [Fact]
    public void Lerp_BelowZero_ClampsToZero()
    {
        var a = new Vector2(0f, 0f);
        var b = new Vector2(10f, 10f);

        var result = Vector2.Lerp(a, b, -0.5f);

        result.X.Should().Be(0f);
        result.Y.Should().Be(0f);
    }

    [Fact]
    public void Lerp_AboveOne_ClampsToOne()
    {
        var a = new Vector2(0f, 0f);
        var b = new Vector2(10f, 10f);

        var result = Vector2.Lerp(a, b, 1.5f);

        result.X.Should().Be(10f);
        result.Y.Should().Be(10f);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var vec = new Vector2(3.456f, 7.89f);

        var str = vec.ToString();

        str.Should().Be("(3.46, 7.89)");
    }

    // Integration scenarios
    [Fact]
    public void Scenario_VelocityApplication_Works()
    {
        var position = new Vector2(100f, 100f);
        var velocity = new Vector2(50f, 0f);
        float deltaTime = 1f;

        var newPosition = position + (velocity * deltaTime);

        newPosition.X.Should().Be(150f);
        newPosition.Y.Should().Be(100f);
    }

    [Fact]
    public void Scenario_MovingTowardsTarget_Works()
    {
        var start = new Vector2(0f, 0f);
        var target = new Vector2(100f, 0f);
        float progress = 0.25f;

        var current = Vector2.Lerp(start, target, progress);

        current.X.Should().Be(25f);
        current.Y.Should().Be(0f);
    }

    [Fact]
    public void Scenario_CheckingIfReachedTarget_Works()
    {
        var position = new Vector2(100f, 100f);
        var target = new Vector2(105f, 105f);
        float threshold = 10f;

        var distance = Vector2.Distance(position, target);
        bool reached = distance <= threshold;

        reached.Should().BeTrue();
    }
}

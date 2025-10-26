using CookingProject.Logic.Features.Cooking.Logic;
using FluentAssertions;
using Xunit;

namespace CookingProject.Logic.Tests.Features.Cooking.Logic;

/// <summary>
/// Tests for pure heat source business logic with no ECS dependencies.
/// These tests are fast, simple, and don't require World management.
/// </summary>
public class HeatLogicTests
{
    #region IsValidHeatLevel Tests

    [Theory]
    [InlineData(0.0f, true)]
    [InlineData(0.33f, true)]
    [InlineData(0.66f, true)]
    [InlineData(1.0f, true)]
    public void IsValidHeatLevel_ValidLevels_ReturnsTrue(float heatLevel, bool expected)
    {
        // Act
        bool result = HeatLogic.IsValidHeatLevel(heatLevel);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(0.1f)]
    [InlineData(1.5f)]
    [InlineData(-0.1f)]
    [InlineData(0.2f)]
    public void IsValidHeatLevel_InvalidLevels_ReturnsFalse(float heatLevel)
    {
        // Act
        bool result = HeatLogic.IsValidHeatLevel(heatLevel);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidHeatLevel_NearLow_WithinTolerance_ReturnsTrue()
    {
        // Arrange - Slightly off from 0.33 due to floating point math
        float heatLevel = 0.3299f;

        // Act
        bool result = HeatLogic.IsValidHeatLevel(heatLevel);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidHeatLevel_NearMedium_WithinTolerance_ReturnsTrue()
    {
        // Arrange
        float heatLevel = 0.6601f;

        // Act
        bool result = HeatLogic.IsValidHeatLevel(heatLevel);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region SnapToValidHeatLevel Tests

    [Fact]
    public void SnapToValidHeatLevel_VeryLow_SnapsToOff()
    {
        // Arrange
        float heatLevel = 0.1f;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert
        result.Should().Be(HeatLogic.HeatOff);
    }

    [Fact]
    public void SnapToValidHeatLevel_JustAboveLow_SnapsToLow()
    {
        // Arrange
        float heatLevel = 0.3f;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert
        result.Should().BeApproximately(HeatLogic.HeatLow, 0.01f);
    }

    [Fact]
    public void SnapToValidHeatLevel_BetweenLowAndMedium_SnapsToNearest()
    {
        // Arrange - Closer to low (0.33)
        float heatLevel = 0.4f;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert
        result.Should().BeApproximately(HeatLogic.HeatLow, 0.01f);
    }

    [Fact]
    public void SnapToValidHeatLevel_BetweenMediumAndHigh_SnapsToNearest()
    {
        // Arrange - Closer to high (1.0)
        float heatLevel = 0.9f;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert
        result.Should().Be(HeatLogic.HeatHigh);
    }

    [Fact]
    public void SnapToValidHeatLevel_VeryHigh_SnapsToHigh()
    {
        // Arrange
        float heatLevel = 1.5f;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert
        result.Should().Be(HeatLogic.HeatHigh);
    }

    [Fact]
    public void SnapToValidHeatLevel_Negative_SnapsToOff()
    {
        // Arrange
        float heatLevel = -0.5f;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert
        result.Should().Be(HeatLogic.HeatOff);
    }

    [Fact]
    public void SnapToValidHeatLevel_ExactlyMidpoint_SnapsToLower()
    {
        // Arrange - Exactly between Low (0.33) and Medium (0.66)
        float heatLevel = (HeatLogic.HeatLow + HeatLogic.HeatMedium) / 2;

        // Act
        float result = HeatLogic.SnapToValidHeatLevel(heatLevel);

        // Assert - Midpoint snaps to lower due to <= comparison
        result.Should().BeApproximately(HeatLogic.HeatLow, 0.01f);
    }

    #endregion

    #region IsHeatInOptimalRange Tests

    [Fact]
    public void IsHeatInOptimalRange_WithinRange_ReturnsTrue()
    {
        // Arrange
        float currentHeat = 0.66f; // Medium
        float optimalMin = 0.33f;  // Low
        float optimalMax = 1.0f;   // High

        // Act
        bool result = HeatLogic.IsHeatInOptimalRange(currentHeat, optimalMin, optimalMax);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHeatInOptimalRange_ExactlyAtMin_ReturnsTrue()
    {
        // Arrange
        float currentHeat = 0.33f;
        float optimalMin = 0.33f;
        float optimalMax = 1.0f;

        // Act
        bool result = HeatLogic.IsHeatInOptimalRange(currentHeat, optimalMin, optimalMax);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHeatInOptimalRange_ExactlyAtMax_ReturnsTrue()
    {
        // Arrange
        float currentHeat = 1.0f;
        float optimalMin = 0.33f;
        float optimalMax = 1.0f;

        // Act
        bool result = HeatLogic.IsHeatInOptimalRange(currentHeat, optimalMin, optimalMax);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHeatInOptimalRange_BelowMin_ReturnsFalse()
    {
        // Arrange
        float currentHeat = 0.0f;  // Off
        float optimalMin = 0.33f;
        float optimalMax = 1.0f;

        // Act
        bool result = HeatLogic.IsHeatInOptimalRange(currentHeat, optimalMin, optimalMax);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsHeatInOptimalRange_AboveMax_ReturnsFalse()
    {
        // Arrange
        float currentHeat = 1.0f;  // High
        float optimalMin = 0.33f;
        float optimalMax = 0.66f;  // Medium max

        // Act
        bool result = HeatLogic.IsHeatInOptimalRange(currentHeat, optimalMin, optimalMax);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsHeatInOptimalRange_NarrowRange_OnlyOneLevel()
    {
        // Arrange - Food that needs exactly Medium heat
        float currentHeat = 0.66f;
        float optimalMin = 0.66f;
        float optimalMax = 0.66f;

        // Act
        bool result = HeatLogic.IsHeatInOptimalRange(currentHeat, optimalMin, optimalMax);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Integration/Realistic Scenarios

    [Fact]
    public void Scenario_PlayerAdjustsHeat_FromLowToHigh()
    {
        // Arrange & Act - Player increases to high
        float newHeat = HeatLogic.HeatHigh;
        bool isValid = HeatLogic.IsValidHeatLevel(newHeat);

        // Assert
        isValid.Should().BeTrue();
        newHeat.Should().Be(1.0f);
    }

    [Fact]
    public void Scenario_InvalidInput_GetsCorrected()
    {
        // Arrange - Player input or physics system gives invalid heat
        float invalidHeat = 0.75f;

        // Act
        float corrected = HeatLogic.SnapToValidHeatLevel(invalidHeat);
        bool isValid = HeatLogic.IsValidHeatLevel(corrected);

        // Assert - 0.75 is closer to Medium (0.66) than High (1.0)
        isValid.Should().BeTrue();
        corrected.Should().BeApproximately(HeatLogic.HeatMedium, 0.01f);
    }

    [Fact]
    public void Scenario_EggCooking_RequiresMediumHeat()
    {
        // Arrange - Eggs need medium heat (0.66)
        float optimalMin = HeatLogic.HeatMedium;
        float optimalMax = HeatLogic.HeatMedium;

        // Act & Assert - Different heat levels
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatOff, optimalMin, optimalMax).Should().BeFalse();
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatLow, optimalMin, optimalMax).Should().BeFalse();
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatMedium, optimalMin, optimalMax).Should().BeTrue();
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatHigh, optimalMin, optimalMax).Should().BeFalse();
    }

    [Fact]
    public void Scenario_SteakCooking_AcceptsWideRange()
    {
        // Arrange - Steak accepts Low to High
        float optimalMin = HeatLogic.HeatLow;
        float optimalMax = HeatLogic.HeatHigh;

        // Act & Assert
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatOff, optimalMin, optimalMax).Should().BeFalse();
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatLow, optimalMin, optimalMax).Should().BeTrue();
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatMedium, optimalMin, optimalMax).Should().BeTrue();
        HeatLogic.IsHeatInOptimalRange(HeatLogic.HeatHigh, optimalMin, optimalMax).Should().BeTrue();
    }

    #endregion
}

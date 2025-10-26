using CookingProject.Logic.Features.Cooking.Logic;
using FluentAssertions;
using Xunit;

namespace CookingProject.Logic.Tests.Features.Cooking.Logic;

/// <summary>
/// Tests for pure cooking business logic with no ECS dependencies.
/// These tests are fast, simple, and don't require World management.
/// </summary>
public class CookingLogicTests
{
    #region CalculateCookingProgress Tests

    [Fact]
    public void CalculateCookingProgress_HighHeat_CooksFaster()
    {
        // Arrange
        float currentHeat = 1.0f; // High
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);

        // Assert
        result.Should().BeApproximately(0.1f, 0.001f); // 1.0/10 = 0.1 per second
    }

    [Fact]
    public void CalculateCookingProgress_MediumHeat_CooksSlower()
    {
        // Arrange
        float currentHeat = 0.66f; // Medium
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);

        // Assert
        result.Should().BeApproximately(0.066f, 0.001f); // 0.66/10 = 0.066 per second
    }

    [Fact]
    public void CalculateCookingProgress_LowHeat_CooksSlowly()
    {
        // Arrange
        float currentHeat = 0.33f; // Low
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);

        // Assert
        result.Should().BeApproximately(0.033f, 0.001f); // 0.33/10 = 0.033 per second
    }

    [Fact]
    public void CalculateCookingProgress_NoHeat_ReturnsZero()
    {
        // Arrange
        float currentHeat = 0.0f; // Off
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateCookingProgress_ZeroCookTime_ReturnsZero()
    {
        // Arrange - Edge case
        float currentHeat = 1.0f;
        float cookTimeSeconds = 0.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateCookingProgress_SmallerDeltaTime_ProportionalProgress()
    {
        // Arrange - 60 fps frame time
        float currentHeat = 1.0f;
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f / 60.0f;

        // Act
        float result = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);

        // Assert
        result.Should().BeApproximately(0.00167f, 0.00001f); // (1.0/10)/60
    }

    #endregion

    #region CalculateCoolingProgress Tests

    [Fact]
    public void CalculateCoolingProgress_Returns30PercentOfCookingSpeed()
    {
        // Arrange
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCoolingProgress(cookTimeSeconds, deltaTime);

        // Assert - Cooling is 30% of base cooking rate
        float baseCookingRate = 1.0f / cookTimeSeconds; // 0.1
        float expectedCoolingRate = baseCookingRate * 0.3f; // 0.03
        result.Should().BeApproximately(expectedCoolingRate, 0.001f);
    }

    [Fact]
    public void CalculateCoolingProgress_ZeroCookTime_ReturnsZero()
    {
        // Arrange
        float cookTimeSeconds = 0.0f;
        float deltaTime = 1.0f;

        // Act
        float result = CookingLogic.CalculateCoolingProgress(cookTimeSeconds, deltaTime);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateCoolingProgress_SmallerDeltaTime_ProportionalCooling()
    {
        // Arrange
        float cookTimeSeconds = 10.0f;
        float deltaTime = 0.5f;

        // Act
        float result = CookingLogic.CalculateCoolingProgress(cookTimeSeconds, deltaTime);

        // Assert
        result.Should().BeApproximately(0.015f, 0.001f); // (0.1 * 0.3) * 0.5
    }

    #endregion

    #region ApplyDonenessChange Tests

    [Fact]
    public void ApplyDonenessChange_AddingProgress_IncreasesCorrectly()
    {
        // Arrange
        float currentDoneness = 0.5f;
        float progressAmount = 0.2f;

        // Act
        float result = CookingLogic.ApplyDonenessChange(currentDoneness, progressAmount);

        // Assert
        result.Should().Be(0.7f);
    }

    [Fact]
    public void ApplyDonenessChange_SubtractingProgress_DecreasesCorrectly()
    {
        // Arrange
        float currentDoneness = 0.5f;
        float progressAmount = -0.2f; // Cooling

        // Act
        float result = CookingLogic.ApplyDonenessChange(currentDoneness, progressAmount);

        // Assert
        result.Should().Be(0.3f);
    }

    [Fact]
    public void ApplyDonenessChange_ClampsToMinimum()
    {
        // Arrange
        float currentDoneness = 0.1f;
        float progressAmount = -0.5f; // Would go negative

        // Act
        float result = CookingLogic.ApplyDonenessChange(currentDoneness, progressAmount);

        // Assert
        result.Should().Be(0.0f); // Clamped to min
    }

    [Fact]
    public void ApplyDonenessChange_ClampsToMaximum()
    {
        // Arrange
        float currentDoneness = 1.9f;
        float progressAmount = 0.5f; // Would exceed 2.0

        // Act
        float result = CookingLogic.ApplyDonenessChange(currentDoneness, progressAmount);

        // Assert
        result.Should().Be(CookingLogic.MaxDoneness); // Clamped to 2.0
    }

    #endregion

    #region IsPerfectlyCooked Tests

    [Theory]
    [InlineData(0.0f, false)]
    [InlineData(0.5f, false)]
    [InlineData(0.99f, false)]
    [InlineData(1.0f, true)]
    [InlineData(1.1f, true)]
    [InlineData(2.0f, true)]
    public void IsPerfectlyCooked_VariousDoneness_ReturnsExpected(float doneness, bool expected)
    {
        // Act
        bool result = CookingLogic.IsPerfectlyCooked(doneness);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region IsBurning Tests

    [Theory]
    [InlineData(0.0f, false)]
    [InlineData(0.5f, false)]
    [InlineData(1.0f, false)]
    [InlineData(1.01f, true)]
    [InlineData(1.5f, true)]
    [InlineData(2.0f, true)]
    public void IsBurning_VariousDoneness_ReturnsExpected(float doneness, bool expected)
    {
        // Act
        bool result = CookingLogic.IsBurning(doneness);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region CalculateBurnLevel Tests

    [Fact]
    public void CalculateBurnLevel_NotBurning_ReturnsZero()
    {
        // Arrange
        float doneness = 0.5f;

        // Act
        float result = CookingLogic.CalculateBurnLevel(doneness);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateBurnLevel_ExactlyPerfect_ReturnsZero()
    {
        // Arrange
        float doneness = 1.0f;

        // Act
        float result = CookingLogic.CalculateBurnLevel(doneness);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateBurnLevel_JustStartedBurning_ReturnsSmallValue()
    {
        // Arrange
        float doneness = 1.1f;

        // Act
        float result = CookingLogic.CalculateBurnLevel(doneness);

        // Assert
        result.Should().BeApproximately(0.1f, 0.001f); // (1.1-1.0)/(2.0-1.0) = 0.1
    }

    [Fact]
    public void CalculateBurnLevel_HalfwayBurnt_ReturnsHalf()
    {
        // Arrange
        float doneness = 1.5f;

        // Act
        float result = CookingLogic.CalculateBurnLevel(doneness);

        // Assert
        result.Should().BeApproximately(0.5f, 0.001f); // (1.5-1.0)/(2.0-1.0) = 0.5
    }

    [Fact]
    public void CalculateBurnLevel_CompletelyBurnt_ReturnsOne()
    {
        // Arrange
        float doneness = 2.0f;

        // Act
        float result = CookingLogic.CalculateBurnLevel(doneness);

        // Assert
        result.Should().Be(1.0f); // (2.0-1.0)/(2.0-1.0) = 1.0
    }

    #endregion

    #region CalculateQuality Tests

    [Fact]
    public void CalculateQuality_PerfectDoneness_ReturnsOne()
    {
        // Arrange
        float doneness = 1.0f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().Be(1.0f);
    }

    [Fact]
    public void CalculateQuality_Raw_ReturnsZero()
    {
        // Arrange
        float doneness = 0.0f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateQuality_CompletelyBurnt_ReturnsZero()
    {
        // Arrange
        float doneness = 2.0f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateQuality_SlightlyUndercooked_HighQuality()
    {
        // Arrange
        float doneness = 0.9f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().BeApproximately(0.9f, 0.001f); // Distance from perfect: 0.1/1.0 = 0.1 penalty
    }

    [Fact]
    public void CalculateQuality_SlightlyOvercooked_HighQuality()
    {
        // Arrange
        float doneness = 1.1f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().BeApproximately(0.9f, 0.001f); // Distance from perfect: 0.1/1.0 = 0.1 penalty
    }

    [Fact]
    public void CalculateQuality_HalfCooked_MediumQuality()
    {
        // Arrange
        float doneness = 0.5f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().BeApproximately(0.5f, 0.001f); // Distance from perfect: 0.5/1.0 = 0.5 penalty
    }

    [Fact]
    public void CalculateQuality_HalfwayBurnt_MediumQuality()
    {
        // Arrange
        float doneness = 1.5f;

        // Act
        float result = CookingLogic.CalculateQuality(doneness);

        // Assert
        result.Should().BeApproximately(0.5f, 0.001f); // Distance from perfect: 0.5/1.0 = 0.5 penalty
    }

    #endregion

    #region CalculateProgressPercent Tests

    [Theory]
    [InlineData(0.0f, 0.0f)]
    [InlineData(0.25f, 0.25f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(0.75f, 0.75f)]
    [InlineData(1.0f, 1.0f)]
    [InlineData(1.5f, 1.0f)] // Clamped to 1.0
    [InlineData(2.0f, 1.0f)] // Clamped to 1.0
    public void CalculateProgressPercent_VariousDoneness_ReturnsExpected(float doneness, float expected)
    {
        // Act
        float result = CookingLogic.CalculateProgressPercent(doneness);

        // Assert
        result.Should().BeApproximately(expected, 0.001f);
    }

    #endregion

    #region Integration/Realistic Scenarios

    [Fact]
    public void Scenario_CookingOnHighHeat_ReachesPerfectionIn10Seconds()
    {
        // Arrange - Simulate at 1 second intervals for clarity
        float doneness = 0.0f;
        float currentHeat = 1.0f; // High
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act - Simulate 10 seconds of cooking
        for (int second = 0; second < 10; second++)
        {
            float cookingProgress = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);
            doneness = CookingLogic.ApplyDonenessChange(doneness, cookingProgress);
        }

        // Assert
        doneness.Should().BeApproximately(1.0f, 0.001f);
        CookingLogic.IsPerfectlyCooked(doneness).Should().BeTrue();
        CookingLogic.CalculateQuality(doneness).Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void Scenario_CookingOnMediumHeat_TakesLonger()
    {
        // Arrange - Medium heat takes proportionally longer
        float doneness = 0.0f;
        float currentHeat = 0.66f; // Medium
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act - Cook for 15 seconds (should be ~66% done since medium is 0.66 heat)
        for (int second = 0; second < 15; second++)
        {
            float cookingProgress = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);
            doneness = CookingLogic.ApplyDonenessChange(doneness, cookingProgress);
        }

        // Assert - At 0.66 heat, 15 seconds should give ~0.99 doneness
        doneness.Should().BeApproximately(0.99f, 0.01f);
    }

    [Fact]
    public void Scenario_Overcooking_FoodBurns()
    {
        // Arrange
        float doneness = 1.0f; // Already perfect
        float currentHeat = 1.0f; // High
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act - Continue cooking for 5 more seconds
        for (int second = 0; second < 5; second++)
        {
            float cookingProgress = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);
            doneness = CookingLogic.ApplyDonenessChange(doneness, cookingProgress);
        }

        // Assert
        doneness.Should().BeApproximately(1.5f, 0.01f);
        CookingLogic.IsBurning(doneness).Should().BeTrue();
        CookingLogic.CalculateBurnLevel(doneness).Should().BeApproximately(0.5f, 0.01f);
        CookingLogic.CalculateQuality(doneness).Should().BeApproximately(0.5f, 0.01f);
    }

    [Fact]
    public void Scenario_CookThenRemove_FoodCools()
    {
        // Arrange - Cook to halfway
        float doneness = 0.5f;
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act - Remove from heat and let cool for 5 seconds
        for (int second = 0; second < 5; second++)
        {
            float coolingProgress = CookingLogic.CalculateCoolingProgress(cookTimeSeconds, deltaTime);
            doneness = CookingLogic.ApplyDonenessChange(doneness, -coolingProgress);
        }

        // Assert - Cooling at 30% rate: 5 * 0.03 = 0.15 reduction
        doneness.Should().BeApproximately(0.35f, 0.01f);
        doneness.Should().BeGreaterThan(0.0f); // Doesn't instantly go raw
    }

    [Fact]
    public void Scenario_PerfectTiming_HighQuality()
    {
        // Arrange
        float doneness = 0.0f;
        float currentHeat = 1.0f;
        float cookTimeSeconds = 10.0f;

        // Act - Cook for exactly 10 seconds
        float cookingProgress = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, 10.0f);
        doneness = CookingLogic.ApplyDonenessChange(doneness, cookingProgress);

        // Assert
        doneness.Should().Be(1.0f);
        CookingLogic.CalculateQuality(doneness).Should().Be(1.0f);
        CookingLogic.IsPerfectlyCooked(doneness).Should().BeTrue();
        CookingLogic.IsBurning(doneness).Should().BeFalse();
    }

    [Fact]
    public void Scenario_CompletelyBurnt_ZeroQuality()
    {
        // Arrange - Start at perfect, continue cooking
        float doneness = 1.0f;
        float currentHeat = 1.0f;
        float cookTimeSeconds = 10.0f;

        // Act - Cook for another 10 seconds (burn completely)
        float cookingProgress = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, 10.0f);
        doneness = CookingLogic.ApplyDonenessChange(doneness, cookingProgress);

        // Assert
        doneness.Should().Be(2.0f); // Maxed out
        CookingLogic.CalculateBurnLevel(doneness).Should().Be(1.0f);
        CookingLogic.CalculateQuality(doneness).Should().Be(0.0f);
    }

    [Fact]
    public void Scenario_LowHeat_SlowCooking()
    {
        // Arrange
        float doneness = 0.0f;
        float currentHeat = 0.33f; // Low heat
        float cookTimeSeconds = 10.0f;
        float deltaTime = 1.0f;

        // Act - Cook for 10 seconds at low heat
        for (int second = 0; second < 10; second++)
        {
            float cookingProgress = CookingLogic.CalculateCookingProgress(currentHeat, cookTimeSeconds, deltaTime);
            doneness = CookingLogic.ApplyDonenessChange(doneness, cookingProgress);
        }

        // Assert - Only ~33% cooked after 10 seconds at low heat
        doneness.Should().BeApproximately(0.33f, 0.01f);
        CookingLogic.IsPerfectlyCooked(doneness).Should().BeFalse();
    }

    #endregion
}

using CookingProject.Logic.Logic;
using FluentAssertions;
using Xunit;

namespace CookingProject.Logic.Tests.Logic;

/// <summary>
/// Tests for pure sharpening business logic with no ECS dependencies.
/// These tests are fast, simple, and don't require World management.
/// </summary>
public class SharpeningLogicTests
{
    #region CalculateSharpenAmount Tests

    [Fact]
    public void CalculateSharpenAmount_FromZeroToOne_Over5Seconds_Returns0Point2()
    {
        // Arrange
        float currentLevel = 0.0f;
        float maxLevel = 1.0f;
        float duration = 5.0f;
        float deltaTime = 1.0f;

        // Act
        float result = SharpeningLogic.CalculateSharpenAmount(currentLevel, maxLevel, duration, deltaTime);

        // Assert
        result.Should().BeApproximately(0.2f, 0.001f); // 1/5 = 0.2 per second
    }

    [Fact]
    public void CalculateSharpenAmount_FromHalfToFull_Takes2Point5Seconds()
    {
        // Arrange
        float currentLevel = 0.5f;
        float maxLevel = 1.0f;
        float duration = 5.0f;
        float deltaTime = 2.5f;

        // Act
        float result = SharpeningLogic.CalculateSharpenAmount(currentLevel, maxLevel, duration, deltaTime);

        // Assert
        result.Should().BeApproximately(0.25f, 0.001f); // (0.5/5) * 2.5 = 0.25
    }

    [Fact]
    public void CalculateSharpenAmount_AlreadyAtMax_ReturnsZero()
    {
        // Arrange
        float currentLevel = 1.0f;
        float maxLevel = 1.0f;
        float duration = 5.0f;
        float deltaTime = 1.0f;

        // Act
        float result = SharpeningLogic.CalculateSharpenAmount(currentLevel, maxLevel, duration, deltaTime);

        // Assert
        result.Should().Be(0.0f); // Nothing to sharpen
    }

    [Fact]
    public void CalculateSharpenAmount_UpgradedMaxLevel_CalculatesCorrectly()
    {
        // Arrange - Upgraded knife with 1.5 max level
        float currentLevel = 0.5f;
        float maxLevel = 1.5f;
        float duration = 5.0f;
        float deltaTime = 1.0f;

        // Act
        float result = SharpeningLogic.CalculateSharpenAmount(currentLevel, maxLevel, duration, deltaTime);

        // Assert
        result.Should().BeApproximately(0.2f, 0.001f); // (1.0/5) * 1 = 0.2
    }

    [Fact]
    public void CalculateSharpenAmount_FastSharpening3Seconds()
    {
        // Arrange - Upgraded sharpening stone (3 seconds)
        float currentLevel = 0.0f;
        float maxLevel = 1.0f;
        float duration = 3.0f;
        float deltaTime = 1.0f;

        // Act
        float result = SharpeningLogic.CalculateSharpenAmount(currentLevel, maxLevel, duration, deltaTime);

        // Assert
        result.Should().BeApproximately(0.333f, 0.001f); // 1/3 per second
    }

    #endregion

    #region ApplySharpeningProgress Tests

    [Fact]
    public void ApplySharpeningProgress_AddsAmountCorrectly()
    {
        // Arrange
        float currentLevel = 0.5f;
        float sharpenAmount = 0.2f;
        float maxLevel = 1.0f;

        // Act
        float result = SharpeningLogic.ApplySharpeningProgress(currentLevel, sharpenAmount, maxLevel);

        // Assert
        result.Should().Be(0.7f);
    }

    [Fact]
    public void ApplySharpeningProgress_ClampsToMaxLevel()
    {
        // Arrange
        float currentLevel = 0.95f;
        float sharpenAmount = 0.1f; // Would go to 1.05
        float maxLevel = 1.0f;

        // Act
        float result = SharpeningLogic.ApplySharpeningProgress(currentLevel, sharpenAmount, maxLevel);

        // Assert
        result.Should().Be(1.0f); // Clamped to max
    }

    [Fact]
    public void ApplySharpeningProgress_UpgradedMaxLevel_AllowsHigherValues()
    {
        // Arrange
        float currentLevel = 1.2f;
        float sharpenAmount = 0.2f;
        float maxLevel = 1.5f; // Upgraded

        // Act
        float result = SharpeningLogic.ApplySharpeningProgress(currentLevel, sharpenAmount, maxLevel);

        // Assert
        result.Should().BeApproximately(1.4f, 0.001f); // Not clamped yet
    }

    [Fact]
    public void ApplySharpeningProgress_ExactlyAtMax_RemainsAtMax()
    {
        // Arrange
        float currentLevel = 1.0f;
        float sharpenAmount = 0.0f;
        float maxLevel = 1.0f;

        // Act
        float result = SharpeningLogic.ApplySharpeningProgress(currentLevel, sharpenAmount, maxLevel);

        // Assert
        result.Should().Be(1.0f);
    }

    #endregion

    #region IsComplete Tests

    [Fact]
    public void IsComplete_ExactlyAtDuration_ReturnsTrue()
    {
        // Arrange
        float elapsedTime = 5.0f;
        float duration = 5.0f;

        // Act
        bool result = SharpeningLogic.IsComplete(elapsedTime, duration);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_PastDuration_ReturnsTrue()
    {
        // Arrange
        float elapsedTime = 5.1f;
        float duration = 5.0f;

        // Act
        bool result = SharpeningLogic.IsComplete(elapsedTime, duration);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_BeforeDuration_ReturnsFalse()
    {
        // Arrange
        float elapsedTime = 4.9f;
        float duration = 5.0f;

        // Act
        bool result = SharpeningLogic.IsComplete(elapsedTime, duration);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_JustStarted_ReturnsFalse()
    {
        // Arrange
        float elapsedTime = 0.0f;
        float duration = 5.0f;

        // Act
        bool result = SharpeningLogic.IsComplete(elapsedTime, duration);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CalculateProgressPercent Tests

    [Fact]
    public void CalculateProgressPercent_Halfway_Returns0Point5()
    {
        // Arrange
        float elapsedTime = 2.5f;
        float duration = 5.0f;

        // Act
        float result = SharpeningLogic.CalculateProgressPercent(elapsedTime, duration);

        // Assert
        result.Should().BeApproximately(0.5f, 0.001f);
    }

    [Fact]
    public void CalculateProgressPercent_Quarter_Returns0Point25()
    {
        // Arrange
        float elapsedTime = 1.25f;
        float duration = 5.0f;

        // Act
        float result = SharpeningLogic.CalculateProgressPercent(elapsedTime, duration);

        // Assert
        result.Should().BeApproximately(0.25f, 0.001f);
    }

    [Fact]
    public void CalculateProgressPercent_Complete_Returns1()
    {
        // Arrange
        float elapsedTime = 5.0f;
        float duration = 5.0f;

        // Act
        float result = SharpeningLogic.CalculateProgressPercent(elapsedTime, duration);

        // Assert
        result.Should().Be(1.0f);
    }

    [Fact]
    public void CalculateProgressPercent_JustStarted_Returns0()
    {
        // Arrange
        float elapsedTime = 0.0f;
        float duration = 5.0f;

        // Act
        float result = SharpeningLogic.CalculateProgressPercent(elapsedTime, duration);

        // Assert
        result.Should().Be(0.0f);
    }

    [Fact]
    public void CalculateProgressPercent_ZeroDuration_Returns1()
    {
        // Arrange - Edge case: instant completion
        float elapsedTime = 0.0f;
        float duration = 0.0f;

        // Act
        float result = SharpeningLogic.CalculateProgressPercent(elapsedTime, duration);

        // Assert
        result.Should().Be(1.0f); // Already complete
    }

    #endregion

    #region Integration/Realistic Scenarios

    [Fact]
    public void Scenario_FullSharpening_From0To1_Over60Frames()
    {
        // Arrange - Simulate 60 fps for 5 seconds
        float initialLevel = 0.0f;
        float currentLevel = 0.0f;
        float maxLevel = 1.0f;
        float duration = 5.0f;
        float deltaTime = 1.0f / 60.0f; // ~16.67ms per frame

        // Act - Simulate 300 frames (5 seconds * 60 fps)
        for (int frame = 0; frame < 300; frame++)
        {
            float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(
                initialLevel, maxLevel, duration, deltaTime);
            currentLevel = SharpeningLogic.ApplySharpeningProgress(
                currentLevel, sharpenAmount, maxLevel);
        }

        // Assert
        currentLevel.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void Scenario_PartialSharpening_ThenCancel_ThenRestart()
    {
        // Arrange
        float initialSharpness = 0.3f;
        float sharpness = 0.3f;
        float maxLevel = 1.0f;
        float duration = 5.0f;

        // Act - Sharpen for 2.5 seconds
        float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(initialSharpness, maxLevel, duration, 2.5f);
        sharpness = SharpeningLogic.ApplySharpeningProgress(sharpness, sharpenAmount, maxLevel);
        float partialSharpness = sharpness;

        // Cancel (sharpness remains at partial level)

        // Restart sharpening from partial level (new initial level)
        float newInitialLevel = sharpness;
        float newSharpenAmount = SharpeningLogic.CalculateSharpenAmount(newInitialLevel, maxLevel, duration, 5.0f);
        sharpness = SharpeningLogic.ApplySharpeningProgress(sharpness, newSharpenAmount, maxLevel);

        // Assert
        partialSharpness.Should().BeApproximately(0.65f, 0.01f); // 0.3 + (0.7/5)*2.5
        sharpness.Should().Be(1.0f); // Completed
    }

    [Fact]
    public void Scenario_UpgradedKnife_SharpensTo1Point5()
    {
        // Arrange - Upgraded knife
        float sharpness = 0.8f;
        float maxLevel = 1.5f; // Upgraded
        float duration = 5.0f;

        // Act - Sharpen to completion
        float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(sharpness, maxLevel, duration, 5.0f);
        sharpness = SharpeningLogic.ApplySharpeningProgress(sharpness, sharpenAmount, maxLevel);

        // Assert
        sharpness.Should().Be(1.5f);
    }

    [Fact]
    public void Scenario_FastSharpening_UpgradedStone()
    {
        // Arrange - Upgraded sharpening stone (3 seconds instead of 5)
        float sharpness = 0.0f;
        float maxLevel = 1.0f;
        float duration = 3.0f;

        // Act
        float sharpenAmount = SharpeningLogic.CalculateSharpenAmount(sharpness, maxLevel, duration, 3.0f);
        sharpness = SharpeningLogic.ApplySharpeningProgress(sharpness, sharpenAmount, maxLevel);

        // Assert
        sharpness.Should().Be(1.0f); // Completed in 3 seconds
    }

    #endregion
}

using CookingProject.Logic.Features.Chopping.Logic;
using Xunit;

namespace CookingProject.Logic.Tests.Features.Chopping.Logic;

/// <summary>
/// Tests for ChoppingLogic pure functions.
/// </summary>
public class ChoppingLogicTests
{
    // ========================================
    // CalculateChopTime Tests
    // ========================================

    [Fact]
    public void CalculateChopTime_SharpKnife_ReturnsBaseTime()
    {
        // Sharp knife (1.0) should take exactly base time
        float chopTime = ChoppingLogic.CalculateChopTime(2.0f, 1.0f);
        Assert.Equal(2.0f, chopTime, precision: 2);
    }

    [Fact]
    public void CalculateChopTime_MediumSharpness_TakesLonger()
    {
        // Medium sharpness (0.5) should take ~54% longer
        float chopTime = ChoppingLogic.CalculateChopTime(2.0f, 0.5f);
        Assert.InRange(chopTime, 3.0f, 3.15f); // Should be ~3.08s
    }

    [Fact]
    public void CalculateChopTime_DullKnife_TakesSignificantlyLonger()
    {
        // Dull knife (0.0) should take ~233% longer
        float chopTime = ChoppingLogic.CalculateChopTime(2.0f, 0.0f);
        Assert.InRange(chopTime, 6.6f, 6.7f); // Should be ~6.67s
    }

    [Fact]
    public void CalculateChopTime_NegativeSharpness_TreatedAsZero()
    {
        // Negative sharpness should be clamped to 0
        float chopTime = ChoppingLogic.CalculateChopTime(2.0f, -0.5f);
        float expectedTime = ChoppingLogic.CalculateChopTime(2.0f, 0.0f);
        Assert.Equal(expectedTime, chopTime, precision: 2);
    }

    [Fact]
    public void CalculateChopTime_VerySharpKnife_FasterThanBase()
    {
        // Extra sharp knife (1.5) should be faster than base
        float chopTime = ChoppingLogic.CalculateChopTime(2.0f, 1.5f);
        Assert.True(chopTime < 2.0f);
    }

    [Fact]
    public void CalculateChopTime_SoftIngredient_FastWithSharpKnife()
    {
        // Soft ingredient (0.8s base) with sharp knife
        float chopTime = ChoppingLogic.CalculateChopTime(0.8f, 1.0f);
        Assert.Equal(0.8f, chopTime, precision: 2);
    }

    [Fact]
    public void CalculateChopTime_SoftIngredient_SlowWithDullKnife()
    {
        // Soft ingredient (0.8s base) with dull knife (0.2 sharpness)
        float chopTime = ChoppingLogic.CalculateChopTime(0.8f, 0.2f);
        Assert.InRange(chopTime, 1.8f, 1.9f);
    }

    // ========================================
    // CalculateProgress Tests
    // ========================================

    [Fact]
    public void CalculateProgress_JustStarted_ReturnsZero()
    {
        float progress = ChoppingLogic.CalculateProgress(0.0f, 2.0f);
        Assert.Equal(0.0f, progress);
    }

    [Fact]
    public void CalculateProgress_Halfway_ReturnsHalf()
    {
        float progress = ChoppingLogic.CalculateProgress(1.0f, 2.0f);
        Assert.Equal(0.5f, progress);
    }

    [Fact]
    public void CalculateProgress_Complete_ReturnsOne()
    {
        float progress = ChoppingLogic.CalculateProgress(2.0f, 2.0f);
        Assert.Equal(1.0f, progress);
    }

    [Fact]
    public void CalculateProgress_Overtime_ClampsToOne()
    {
        float progress = ChoppingLogic.CalculateProgress(3.0f, 2.0f);
        Assert.Equal(1.0f, progress);
    }

    [Fact]
    public void CalculateProgress_ZeroDuration_ReturnsOne()
    {
        // Instant completion if duration is zero
        float progress = ChoppingLogic.CalculateProgress(0.0f, 0.0f);
        Assert.Equal(1.0f, progress);
    }

    [Fact]
    public void CalculateProgress_QuarterWay_ReturnsQuarter()
    {
        float progress = ChoppingLogic.CalculateProgress(0.5f, 2.0f);
        Assert.Equal(0.25f, progress);
    }

    // ========================================
    // ShouldCompleteChop Tests
    // ========================================

    [Fact]
    public void ShouldCompleteChop_NotYetComplete_ReturnsFalse()
    {
        bool complete = ChoppingLogic.ShouldCompleteChop(1.5f, 2.0f);
        Assert.False(complete);
    }

    [Fact]
    public void ShouldCompleteChop_ExactlyComplete_ReturnsTrue()
    {
        bool complete = ChoppingLogic.ShouldCompleteChop(2.0f, 2.0f);
        Assert.True(complete);
    }

    [Fact]
    public void ShouldCompleteChop_PastDuration_ReturnsTrue()
    {
        bool complete = ChoppingLogic.ShouldCompleteChop(2.5f, 2.0f);
        Assert.True(complete);
    }

    [Fact]
    public void ShouldCompleteChop_JustStarted_ReturnsFalse()
    {
        bool complete = ChoppingLogic.ShouldCompleteChop(0.0f, 2.0f);
        Assert.False(complete);
    }

    // ========================================
    // IsFullyChopped Tests
    // ========================================

    [Fact]
    public void IsFullyChopped_NoChopsYet_ReturnsFalse()
    {
        bool fullyChopped = ChoppingLogic.IsFullyChopped(0, 4);
        Assert.False(fullyChopped);
    }

    [Fact]
    public void IsFullyChopped_PartialChops_ReturnsFalse()
    {
        bool fullyChopped = ChoppingLogic.IsFullyChopped(2, 4);
        Assert.False(fullyChopped);
    }

    [Fact]
    public void IsFullyChopped_ExactlyRequired_ReturnsTrue()
    {
        bool fullyChopped = ChoppingLogic.IsFullyChopped(4, 4);
        Assert.True(fullyChopped);
    }

    [Fact]
    public void IsFullyChopped_MoreThanRequired_ReturnsTrue()
    {
        bool fullyChopped = ChoppingLogic.IsFullyChopped(5, 4);
        Assert.True(fullyChopped);
    }

    [Fact]
    public void IsFullyChopped_OneLess_ReturnsFalse()
    {
        bool fullyChopped = ChoppingLogic.IsFullyChopped(3, 4);
        Assert.False(fullyChopped);
    }

    // ========================================
    // ApplyDegradation Tests
    // ========================================

    [Fact]
    public void ApplyDegradation_NormalDegradation_ReducesSharpness()
    {
        float newSharpness = ChoppingLogic.ApplyDegradation(1.0f, 0.08f);
        Assert.Equal(0.92f, newSharpness, precision: 2);
    }

    [Fact]
    public void ApplyDegradation_CannotGoNegative_ClampsToZero()
    {
        float newSharpness = ChoppingLogic.ApplyDegradation(0.05f, 0.08f);
        Assert.Equal(0.0f, newSharpness);
    }

    [Fact]
    public void ApplyDegradation_ExactlyToZero_ReturnsZero()
    {
        float newSharpness = ChoppingLogic.ApplyDegradation(0.08f, 0.08f);
        Assert.Equal(0.0f, newSharpness);
    }

    [Fact]
    public void ApplyDegradation_SoftIngredient_SmallDegradation()
    {
        float newSharpness = ChoppingLogic.ApplyDegradation(1.0f, 0.03f);
        Assert.Equal(0.97f, newSharpness, precision: 2);
    }

    [Fact]
    public void ApplyDegradation_HardIngredient_LargeDegradation()
    {
        float newSharpness = ChoppingLogic.ApplyDegradation(1.0f, 0.08f);
        Assert.Equal(0.92f, newSharpness, precision: 2);
    }

    [Fact]
    public void ApplyDegradation_AlreadyDull_StaysAtZero()
    {
        float newSharpness = ChoppingLogic.ApplyDegradation(0.0f, 0.08f);
        Assert.Equal(0.0f, newSharpness);
    }

    // ========================================
    // IncrementChops Tests
    // ========================================

    [Fact]
    public void IncrementChops_FromZero_ReturnsOne()
    {
        int newChops = ChoppingLogic.IncrementChops(0, 4);
        Assert.Equal(1, newChops);
    }

    [Fact]
    public void IncrementChops_FromTwo_ReturnsThree()
    {
        int newChops = ChoppingLogic.IncrementChops(2, 4);
        Assert.Equal(3, newChops);
    }

    [Fact]
    public void IncrementChops_AtMax_ClampsToMax()
    {
        int newChops = ChoppingLogic.IncrementChops(4, 4);
        Assert.Equal(4, newChops);
    }

    [Fact]
    public void IncrementChops_PastMax_ClampsToMax()
    {
        // Should never happen, but handles it safely
        int newChops = ChoppingLogic.IncrementChops(5, 4);
        Assert.Equal(4, newChops);
    }

    [Fact]
    public void IncrementChops_OneLess_ReachesMax()
    {
        int newChops = ChoppingLogic.IncrementChops(3, 4);
        Assert.Equal(4, newChops);
    }
}

using Arch.Core;

namespace CookingProject.Logic.Features.Chopping.Components;

/// <summary>
/// Represents an active chopping operation in progress.
/// This component is added when chopping starts and removed when it completes or is cancelled.
/// </summary>
public struct ChoppingProgress
{
    /// <summary>
    /// The knife entity being used for chopping.
    /// Used to access Sharpness component for degradation.
    /// </summary>
    public Entity KnifeEntity;

    /// <summary>
    /// Time elapsed during current chop (in seconds).
    /// Resets to 0 after each chop completes.
    /// </summary>
    public float ElapsedTime;

    /// <summary>
    /// Total time required to complete one chop (in seconds).
    /// Calculated from ingredient base time and knife sharpness.
    /// </summary>
    public float ChopDuration;

    public ChoppingProgress(Entity knifeEntity, float chopDuration)
    {
        KnifeEntity = knifeEntity;
        ElapsedTime = 0f;
        ChopDuration = chopDuration;
    }
}

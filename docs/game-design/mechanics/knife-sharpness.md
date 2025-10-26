# Knife Sharpness Mechanic

## Purpose

Knives are the primary tool for ingredient preparation. Sharpness creates a resource management challenge - players must balance using the knife vs. taking time to sharpen it.

## Sharpness Component

**Data Structure:**
```csharp
struct Sharpness
{
    float Level;        // 0.0 (completely dull) to 1.0 (razor sharp)
    float MaxLevel;     // Maximum sharpness achievable (upgradeable)
}
```

## Degradation Rules

**When Sharpness Degrades:**
- Each successful chop: `-0.05 sharpness`
- Each failed chop (wrong tool for ingredient): `-0.10 sharpness`
- Chopping hard ingredients (carrots, potatoes): `-0.08 sharpness`
- Chopping soft ingredients (tomatoes, herbs): `-0.03 sharpness`

**Degradation Curve:**
- Sharp knives (0.8-1.0): Degrade slowly
- Medium knives (0.4-0.8): Normal degradation
- Dull knives (0.0-0.4): No additional degradation (already dull)

**Minimum Sharpness:**
- Cannot go below 0.0
- At 0.0, knife is "completely dull" but still usable (just slow)

## Impact on Chopping Speed

**Chop Time Calculation:**
```
chopTime = baseChopTime / (0.3 + (sharpness * 0.7))
```

**Examples:**
- Sharpness 1.0 (sharp): 1.0 second base time
- Sharpness 0.5 (medium): 1.54 seconds (+54%)
- Sharpness 0.0 (dull): 3.33 seconds (+233%)

**Why This Formula:**
- Even dull knives work (0.3 multiplier minimum)
- Sharp knives provide significant advantage
- Encourages sharpening but doesn't make dull knives unusable

## Sharpening Process

**How to Sharpen:**
1. Player must stop current task
2. Click/interact with sharpening stone
3. Hold interaction for duration
4. Sharpness restored

**Sharpening Time:**
- Base sharpening duration: 5 seconds
- Can be upgraded: 4s → 3s → 2s
- Cannot be interrupted once started

**Sharpening Progress:**
```
sharpenPerSecond = (maxLevel - currentLevel) / sharpeningDuration
progress += sharpenPerSecond * deltaTime
```

**Restoration:**
- Restores to `MaxLevel` (default 1.0)
- `MaxLevel` can be increased via upgrades (1.0 → 1.2 → 1.5)
- Higher max level provides longer sharp period

## Visual/Audio Feedback

**Sharpness Indicators:**
- **1.0 - 0.8**: Knife sprite has bright gleam, fast chopping sound
- **0.8 - 0.5**: Normal knife sprite, normal chopping sound
- **0.5 - 0.2**: Dulled sprite (less reflective), slower/heavier chop sound
- **0.2 - 0.0**: Very dull sprite, labored chopping sound + visual struggle

**Sharpening Feedback:**
- Scraping sound during sharpening
- Particle effect (sparks from stone)
- Sharpness meter fills up visually
- Satisfying "ding" sound when complete

## Strategic Considerations

**When to Sharpen:**
- **Good time**: Between orders, during downtime
- **Bad time**: During rush, with pending orders
- **Emergency**: Accept slower chopping if sharpening would make order late

**Skill Expression:**
- Expert players sharpen proactively during lulls
- Novice players sharpen reactively when knife is completely dull
- Optimal play: Sharpen at ~0.4-0.5 to avoid "completely dull" penalty

## Events

**Events Emitted:**
- `KnifeDegradedEvent(entityId, newSharpness)` - After each chop
- `SharpeningStartedEvent(entityId)` - Player starts sharpening
- `SharpeningProgressEvent(entityId, progress)` - During sharpening
- `KnifeSharpenedEvent(entityId, finalSharpness)` - Sharpening complete
- `KnifeTooD ullWarningEvent(entityId)` - When sharpness falls below 0.3

## Future Enhancements

- **Multiple knife types**: Different degradation rates, different base speeds
- **Honing vs Sharpening**: Quick honing (1s, +0.2) vs full sharpen (5s, to max)
- **Sharpening quality**: Mini-game for perfect sharpen (+0.1 bonus sharpness)
- **Professional knife**: Unlock that degrades slower, stays sharp longer

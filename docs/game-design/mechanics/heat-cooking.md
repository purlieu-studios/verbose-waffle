# Heat & Cooking Mechanic

## Purpose

Cooking is the core gameplay loop where players transform raw ingredients into finished dishes. The heat system creates timing challenges and skill expression through heat management, preventing food from burning while ensuring proper doneness.

## Heat Source Component

**Data Structure:**
```csharp
struct HeatSource
{
    float CurrentHeat;           // 0.0, 0.33, 0.66, or 1.0
    Entity CookingEntity;        // What's currently on the burner
    bool HasCookingEntity;       // Whether burner is occupied
}
```

**Heat Levels:**
- **0.0** - Off (no cooking)
- **0.33** - Low heat
- **0.66** - Medium heat
- **1.0** - High heat

**Heat Behavior:**
- Heat levels are discrete (no drift between levels)
- Player actively controls heat level
- Heat changes instantly when adjusted
- Each burner operates independently

## Cooking Progress Component

**Data Structure:**
```csharp
struct CookingProgress
{
    float Doneness;              // 0.0 (raw) → 1.0 (perfect) → 2.0 (burnt)
    float OptimalHeatMin;        // Minimum optimal heat
    float OptimalHeatMax;        // Maximum optimal heat
    float CookTimeSeconds;       // Time to reach perfect at optimal heat
    bool IsOnHeat;               // Currently on burner
    Entity StoveEntity;          // Which burner (if on heat)
}
```

## Cooking Rules

**Doneness Progression:**
- **0.0** - Raw/Uncooked
- **0.0 → 1.0** - Cooking phase (gradual cooking)
- **1.0** - Perfect doneness (ideal moment to remove)
- **1.0 → 2.0** - Burning phase (quality degradation)
- **2.0** - Completely ruined (inedible)

**Cooking Speed:**
```
cookingRate = currentHeat / cookTimeSeconds
doneness += cookingRate * deltaTime
```

**Examples:**
- High heat (1.0) on 10s food: Cooks in 10 seconds
- Medium heat (0.66) on 10s food: Cooks in ~15 seconds
- Low heat (0.33) on 10s food: Cooks in ~30 seconds

**Cooling (Off Heat):**
- When removed from heat, food cools at 30% of cooking speed
- Cooling prevents overcooking but slowly returns to raw
- Food never goes below 0.0 doneness

```
coolingRate = (1.0 / cookTimeSeconds) * 0.3
doneness -= coolingRate * deltaTime
```

## Optimal Heat Ranges

Different foods require different heat levels for best results:

**Example Foods:**
- **Eggs**: Medium only (0.66 - 0.66)
  - Too low: Very slow cooking
  - Too high: Burns immediately

- **Steak**: Low to High (0.33 - 1.0)
  - Flexible cooking, harder to burn
  - Quality varies with heat precision

- **Soup**: Low to Medium (0.33 - 0.66)
  - Forgiving heat range
  - Difficult to burn

**Cooking Outside Optimal Range:**
- Still cooks food (no "wrong heat = no cooking")
- Lower quality final product
- May cook slower or faster than optimal

## Burning System

**Burn Progress Component:**
```csharp
struct BurnProgress
{
    float BurnLevel;  // 0.0 (just started) → 1.0 (ruined)
}
```

**When Burning Occurs:**
- Food doneness exceeds 1.0
- BurnProgress component automatically added
- Visual/audio warnings begin

**Burn Level Calculation:**
```
burnLevel = (doneness - 1.0) / (2.0 - 1.0)
burnLevel = clamp(burnLevel, 0.0, 1.0)
```

**Burn Stages:**
- **0.0 - 0.3**: Slightly overcooked (edible, reduced quality)
- **0.3 - 0.7**: Burnt (barely edible, significant quality loss)
- **0.7 - 1.0**: Ruined (inedible, customer rejection)

## Quality System

Quality is calculated when food reaches perfect doneness (1.0) or when served:

```
quality = 1.0 - (abs(doneness - 1.0) / max(1.0, 2.0 - 1.0))
quality = clamp(quality, 0.0, 1.0)
```

**Quality Tiers:**
- **1.0**: Perfect! (exactly 1.0 doneness)
- **0.8 - 0.99**: Excellent
- **0.6 - 0.79**: Good
- **0.4 - 0.59**: Acceptable
- **0.2 - 0.39**: Poor
- **0.0 - 0.19**: Ruined

**Quality Impact:**
- Customer satisfaction
- Tip amount
- Restaurant reputation
- Achievement progress

## Container System

**Container Component:**
```csharp
struct Container
{
    ContainerType Type;      // Pan, Pot, etc.
    int? ContainingFoodId;   // What's in the container
}
```

**Container Requirements:**
```csharp
struct CookingRequirements
{
    bool RequiresContainer;
    ContainerType RequiredContainerType;
    float OptimalHeatMin;
    float OptimalHeatMax;
    float CookTimeSeconds;
}
```

**Examples:**
- **Eggs**: Require Pan
- **Steak**: No container (directly on burner)
- **Soup**: Requires Pot

## Visual/Audio Feedback

**Cooking Indicators:**
- **Raw (0.0-0.3)**: Raw food appearance, no sizzling
- **Cooking (0.3-0.9)**: Gradual color change, sizzling sounds
- **Almost Done (0.9-1.0)**: Strong sizzle, steam, visual cues
- **Perfect (1.0)**: Flash effect, "ding" sound, golden appearance
- **Burning (1.0-1.5)**: Smoke particles, burning smell, dark color
- **Ruined (1.5-2.0)**: Heavy smoke, charred appearance, alarm sound

**Heat Level Feedback:**
- Flame height matches heat level
- Burner glow intensity
- Heat shimmer effect
- Ambient sound (low hum to roar)

## Strategic Considerations

**Heat Management:**
- **High heat**: Fast cooking, easy to burn, requires attention
- **Medium heat**: Balanced speed/safety, most versatile
- **Low heat**: Slow cooking, very safe, good for multitasking

**When to Remove from Heat:**
- **Perfect timing**: Remove at exactly 1.0 doneness
- **Play it safe**: Remove at 0.9-0.95, let residual heat finish
- **Emergency**: Remove anytime to prevent burning, can return later

**Multi-burner Management:**
- Start with 1 burner, unlock more later
- Juggle multiple dishes at different stages
- Prioritize attention based on heat levels and doneness

**Skill Expression:**
- Expert players: Perfect doneness on high heat (fast service)
- Novice players: Safe medium/low heat (slower but reliable)
- Optimal play: Match heat to food type and multitasking load

## Events

**Events Emitted:**
- `HeatLevelChangedEvent(burnerId, newHeatLevel)` - Heat adjusted
- `FoodPlacedOnHeatEvent(foodId, burnerId)` - Cooking begins
- `FoodRemovedFromHeatEvent(foodId)` - Removed from heat
- `CookingProgressEvent(foodId, doneness, isInOptimalRange)` - Every frame while cooking
- `BurningStartedEvent(foodId)` - Warning: food burning
- `FoodCookedEvent(foodId, finalDoneness, quality)` - Perfect doneness reached

## Future Enhancements

- **Temperature zones**: Hot/cool spots on burner for advanced control
- **Pan material types**: Cast iron (slow heat/cool), nonstick (fast heat)
- **Heat recovery**: Burners take time to heat up/cool down
- **Induction vs Gas**: Different heat characteristics
- **Sous vide**: Precise temperature control, slow cooking
- **Oven cooking**: Set-and-forget with timer-based cooking
- **Broiler**: High heat from above, different cooking patterns

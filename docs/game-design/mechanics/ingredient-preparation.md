# Ingredient Preparation Mechanic

## Purpose

Ingredients must be prepared (chopped, sliced, diced) before cooking. This mechanic creates time pressure and ties into the knife sharpness system.

## Ingredient Types

### Hard Ingredients
**Examples:** Carrots, Potatoes, Onions

**Properties:**
- Base chop time: 2.0 seconds (at 1.0 sharpness)
- Knife degradation per chop: -0.08 sharpness
- Required chops: 3-4 (depending on recipe)
- Visual states: Whole → Half → Quarters → Diced

### Soft Ingredients
**Examples:** Tomatoes, Herbs, Mushrooms

**Properties:**
- Base chop time: 0.8 seconds (at 1.0 sharpness)
- Knife degradation per chop: -0.03 sharpness
- Required chops: 1-2 (depending on recipe)
- Visual states: Whole → Halved → Chopped

### Medium Ingredients
**Examples:** Bell Peppers, Cucumbers, Zucchini

**Properties:**
- Base chop time: 1.2 seconds (at 1.0 sharpness)
- Knife degradation per chop: -0.05 sharpness
- Required chops: 2-3 (depending on recipe)
- Visual states: Whole → Sliced → Diced

## Chop Time Calculation

**Formula:**
```
chopTime = baseChopTime / (0.3 + (sharpness * 0.7))
```

**Examples (Hard Ingredient, base 2.0s):**
- Sharpness 1.0 (sharp): 2.0 seconds
- Sharpness 0.5 (medium): 3.08 seconds (+54%)
- Sharpness 0.0 (dull): 6.67 seconds (+233%)

**Why This Formula:**
- Even dull knives work (0.3 multiplier minimum prevents infinite time)
- Sharp knives provide significant speed advantage
- Creates compelling reason to sharpen proactively
- Dull knife penalty is severe but not game-breaking

## Ingredient Component

**Data Structure:**
```csharp
struct Ingredient
{
    IngredientType Type;          // Carrot, Tomato, etc.
    IngredientHardness Hardness;  // Hard, Medium, Soft
    float BaseChopTime;           // Time at 1.0 sharpness
    float DegradationAmount;      // How much knife degrades per chop
}
```

**Hardness Enum:**
```csharp
enum IngredientHardness
{
    Soft = 0,    // Tomatoes, herbs
    Medium = 1,  // Peppers, cucumbers
    Hard = 2     // Carrots, potatoes
}
```

## ChoppableItem Component

**Data Structure:**
```csharp
struct ChoppableItem
{
    int RequiredChops;    // How many chops needed (recipe-specific)
    int CurrentChops;     // How many chops completed
    bool IsFullyChopped;  // Completed all required chops
}
```

**Progression:**
```
Carrot (RequiredChops = 4):
  CurrentChops: 0/4 → Whole
  CurrentChops: 1/4 → Halved
  CurrentChops: 2/4 → Quartered
  CurrentChops: 3/4 → Chunked
  CurrentChops: 4/4 → Diced (IsFullyChopped = true)
```

## ChoppingProgress Component

**Data Structure:**
```csharp
struct ChoppingProgress
{
    Entity KnifeEntity;   // Which knife is being used
    float ElapsedTime;    // Progress toward next chop
    float ChopDuration;   // Time required (based on sharpness)
}
```

**Behavior:**
- Added when player starts chopping
- Removed when chop completes OR player cancels
- ElapsedTime accumulates until >= ChopDuration
- On completion: CurrentChops++, degrade knife, check if fully chopped

## Preparation States

**Visual Progression (Carrot Example):**
1. **Whole** - Uncut carrot sprite
2. **Halved** - Carrot cut in half
3. **Quartered** - Carrot in 4 pieces
4. **Diced** - Small cubes, ready for cooking

**State Changes:**
- Each successful chop advances to next state
- Different ingredients have different state counts
- Final state = IsFullyChopped = true
- Only fully chopped ingredients can be cooked

## Strategic Considerations

**When to Sharpen:**
- Before chopping hard ingredients (carrots, potatoes)
- When knife drops below 0.5 (chopping becomes noticeably slower)
- During downtime between orders

**Efficiency Optimization:**
- Batch similar ingredients (all carrots at once)
- Sharpen before hard ingredients, use dull knife for soft ones
- Pre-chop ingredients during slow periods

## Events

**Events Emitted:**
- `ChoppingStartedEvent(ingredientId, knifeId, chopDuration)` - Player begins chopping
- `ChoppingProgressEvent(ingredientId, progress)` - Each frame during chop
- `IngredientChoppedEvent(ingredientId, currentChops, fullyChopped)` - Single chop completes
- `IngredientFullyPreparedEvent(ingredientId)` - All chops complete
- `KnifeDegradedEvent(knifeId, newSharpness)` - Knife dulls from use
- `ChoppingCancelledEvent(ingredientId)` - Player cancels mid-chop

## Integration with Cooking System

**Requirement:**
- Only fully chopped ingredients can be placed on heat
- `PlaceFoodOnBurnerCommand` should check `IsFullyChopped`
- Unchopped ingredients cannot be cooked

**Recipe System (Future):**
- Recipes specify required chop level (Diced, Sliced, Whole)
- Different chop levels affect cooking time/quality
- Sous-vide might accept whole ingredients, sautée requires diced

## Performance Considerations

**Per Frame (60 FPS):**
- Query only entities actively being chopped (~1-3 entities max)
- Progress events every frame = 60/second (acceptable for few ingredients)
- No chop

ping when idle = zero overhead

**Optimization:**
- Use Arch ECS query filters (WithAll<ChoppingProgress>)
- Knife degradation is immediate (no accumulation needed)
- Visual state changes are event-driven (not per-frame checks)

## Testing Strategy

**Unit Tests:**
- Chop time calculation with various sharpness levels
- Knife degradation calculation per ingredient type
- Chop progress tracking and completion
- Multiple chops to reach fully prepared state
- Cannot exceed MaxChops

**Integration Tests:**
- StartChoppingCommand → ChoppingStartedEvent
- Progress accumulation over multiple frames
- Chop completion → IngredientChoppedEvent + KnifeDegradedEvent
- Multiple chops to completion
- Cancelling mid-chop doesn't degrade knife

**Godot Integration Tests:**
- Click ingredient + cutting board starts chopping
- Progress bar updates visually
- Knife sharpness indicator updates
- Ingredient sprite changes on each chop
- Fully chopped ingredient can be picked up/cooked

## Future Enhancements

**Chopping Mini-game:**
- Rhythm-based chopping (press key on beat)
- Perfect timing = faster chops + less degradation
- Failure = wasted time, extra degradation

**Multiple Cutting Styles:**
- Dice vs. Slice vs. Julienne
- Different styles take different time
- Recipes specify required style

**Chopping Skill Upgrades:**
- Faster base chop time
- Less knife degradation
- Auto-complete last chop

**Ingredient Combinations:**
- Chop multiple ingredients together (stir-fry prep)
- Batch bonus: slight time reduction

## Example Scenario

**Player wants to make vegetable soup:**

1. **Gather ingredients:** Carrot (hard), Tomato (soft), Onion (hard)
2. **Check knife:** Sharpness 0.7 (medium-sharp)
3. **Decision:** Chop soft ingredient first (tomato), save hard ones for later
4. **Chop tomato:**
   - Base 0.8s → Actual 1.02s (due to 0.7 sharpness)
   - 1 chop needed → Complete in ~1 second
   - Knife: 0.7 → 0.67 (-0.03)
5. **Sharpen knife:** Restore to 1.0 (takes 5 seconds)
6. **Chop carrot:**
   - Base 2.0s → Actual 2.0s (sharp knife)
   - 4 chops needed → 8 seconds total
   - Knife: 1.0 → 0.68 (-0.08 per chop x 4)
7. **Chop onion:**
   - Base 2.0s → Actual 2.56s (degraded to 0.68)
   - 4 chops needed → ~10 seconds total
8. **Total time:** ~24 seconds (1s + 5s sharpen + 8s + 10s)

**Optimization:**
- If player had sharpened BEFORE starting, total would be ~19s
- Demonstrates strategic value of sharpening proactively

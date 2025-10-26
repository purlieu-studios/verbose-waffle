# Game Design Documentation

This directory contains all game design specifications for the cooking game. These documents define **what** the game should do, while the code in `apps/CookingProject.Logic/` defines **how** it's implemented.

## Purpose

These docs are indexed by the semantic search system. When you ask Claude Code to implement a feature, it will:

1. **Search these docs** to understand what you want
2. **Search the code** to see existing patterns
3. **Combine both** to implement features correctly

## Documentation Structure

### 📄 overview.md
High-level game concept, core loop, win/lose conditions.

**When to reference**: Understanding the big picture, explaining game to others.

### 📁 mechanics/
Detailed specifications for core game mechanics.

**Files:**
- `knife-sharpness.md` - How knife sharpness works, degradation, sharpening
- `temperature-cooking.md` - Temperature system, cooking times, burning (TODO)
- `ingredient-prep.md` - Chopping, slicing, pressing mechanics (TODO)
- `recipe-system.md` - How recipes are defined and completed (TODO)

**When to reference**: Implementing or modifying game mechanics.

### 📁 entities/
Specifications for all game entities (what exists in the world).

**Files:**
- `tools.md` - Knives, presses, sharpening stones (TODO)
- `ingredients.md` - All available ingredients, properties (TODO)
- `stations.md` - Stoves, prep stations, serving areas (TODO)

**When to reference**: Adding new entities or understanding existing ones.

### 📁 systems/
ECS system specifications (what each system does).

**Files:**
- `sharpening-system.md` - SharpeningSystem behavior and events
- `chopping-system.md` - ChoppingSystem behavior (TODO)
- `cooking-system.md` - CookingSystem behavior (existing, needs doc)
- `recipe-system.md` - RecipeSystem for tracking completion (TODO)

**When to reference**: Implementing new systems or modifying existing ones.

### 📁 features/
Planned features and future enhancements.

**Files:**
- `progression.md` - Unlocks, upgrades, difficulty curve (TODO)
- `orders-and-scoring.md` - Order management, scoring system (TODO)
- `multiplayer.md` - Future multiplayer ideas (TODO)

**When to reference**: Planning long-term development.

## How to Use These Docs

### For Developers

**Before implementing a feature:**
1. Read the relevant spec (e.g., `mechanics/knife-sharpness.md`)
2. Check `systems/` for system-level requirements
3. Review `entities/` for entity definitions
4. Implement following the architecture in `apps/CookingProject.Logic/README.md`

**When adding a new feature:**
1. **First**: Document it here (what it should do)
2. **Second**: Commit and reindex
3. **Third**: Implement the code
4. **Fourth**: Update docs if design changed during implementation

### For Claude Code

**When asked to implement something:**
```
You ask: "Implement the sharpening system"

Claude searches:
1. "sharpening system" → finds systems/sharpening-system.md
2. "knife sharpness" → finds mechanics/knife-sharpness.md
3. "System implementation pattern" → finds CookingSystem.cs
4. Combines all three to implement correctly
```

## Documentation Guidelines

### File Size
- **Ideal**: 100-300 lines per file
- **Too small**: <50 lines (combine with related topics)
- **Too large**: >500 lines (split into subtopics)

### Writing Style
- **Be specific**: Include numbers, formulas, exact behavior
- **Use examples**: Show concrete cases
- **Include edge cases**: What happens when things go wrong?
- **Future-proof**: Note potential enhancements for later

### What to Document

✅ **DO document:**
- Exact mechanics (numbers, formulas, ranges)
- Component structure and data types
- System behavior and update logic
- Events and when they fire
- Edge cases and validation rules
- Strategic considerations (how should players use this?)

❌ **DON'T document:**
- Implementation details (that's in code comments)
- Temporary debugging notes (use TODO comments in code)
- Arch ECS patterns (that's in the Arch docs/code)

### Example: Good vs Bad Documentation

**❌ Bad** (too vague):
```markdown
# Knife Sharpness
Knives get dull and need to be sharpened.
Players can sharpen them at the sharpening stone.
```

**✅ Good** (specific, actionable):
```markdown
# Knife Sharpness

Sharpness ranges from 0.0 (dull) to 1.0 (sharp).

Degradation: -0.05 per chop
Chopping time: baseTime / (0.3 + sharpness * 0.7)
Sharpening duration: 5 seconds
Sharpens to: MaxLevel (default 1.0)

Example: At 0.0 sharpness, chopping takes 3.33x longer.
```

## Keeping Docs Up to Date

**When code diverges from spec:**
1. Decide: Is the code wrong, or is the spec wrong?
2. Update the incorrect one
3. Add note explaining the change
4. Reindex so Claude knows the current design

**Version tracking:**
- These docs live in git, tracked like code
- PRs should update docs when changing mechanics
- Treat design changes like code changes (review, discuss)

## Status Legend

- ✅ **Complete**: Fully documented, implemented, tested
- 🚧 **In Progress**: Being implemented or documented
- 📝 **Planned**: Designed but not yet started
- 💡 **Idea**: Rough concept, needs more design

## Current Status

| Feature | Design Doc | Implementation | Status |
|---------|-----------|----------------|--------|
| Knife Sharpness | ✅ mechanics/knife-sharpness.md | 📝 Planned | 🚧 In Progress |
| Cooking System | 📝 Planned | ✅ CookingSystem.cs | 🚧 In Progress |
| Ingredient Chopping | 📝 Planned | 📝 Planned | 💡 Idea |
| Recipe Completion | 📝 Planned | 📝 Planned | 💡 Idea |
| Orders & Scoring | 📝 Planned | 📝 Planned | 💡 Idea |

## Next Steps

1. ✅ Create documentation structure
2. ✅ Document knife sharpness mechanic (example)
3. 📝 Document remaining core mechanics
4. 📝 Define all entities (tools, ingredients, stations)
5. 📝 Specify all systems
6. 📝 Commit and reindex
7. 🚀 Start implementing using Claude Code!

# Pull Request Template Guide

Use this template when creating pull requests to ensure comprehensive documentation and easy review.

## PR Title Format

```
<Type>: <Brief description of the change>
```

### Examples:
- `feat: Implement knife sharpening system with ECS architecture`
- `fix: Correct sharpening rate calculation`
- `refactor: Reorganize codebase into feature-based structure`
- `docs: Add ECS architecture pattern guide`
- `chore: Update dependencies and CI configuration`

## PR Description Template

```markdown
# <Feature/Fix Name>

Brief 1-2 sentence overview of what this PR does.

## 🎯 Overview

More detailed description of the changes:
- What was implemented/fixed
- Why it was needed
- How it solves the problem

## 🏗️ Architecture/Implementation

### Key Design Decisions
Explain major architectural choices made:
- Pattern used (e.g., Pure Logic Separation)
- Why this approach
- How it fits with existing codebase

### Code Structure
```
<Show relevant directory structure or file organization>
```

## 🎮 Features/Changes

List all features or changes included:
- ✅ Feature 1 with brief description
- ✅ Feature 2 with brief description
- ✅ Feature 3 with brief description

## 🧪 Testing

### Test Coverage: <X> tests, <Y>% coverage, <Z>ms
```
✅ Category 1 (N tests)
✅ Category 2 (N tests)
✅ Integration scenarios (N tests)
```

### Sample Test (Optional)
```csharp
<Show a representative test if helpful>
```

## 📦 Components/Files Added

### Core Components
- `Component1` - Description
- `Component2` - Description

### Commands/Events (if applicable)
- `Command1` - Description
- `Event1` - Description

## 📚 Documentation

### Added
- `path/to/doc1.md` - Description (X lines)
- `path/to/doc2.md` - Description (Y lines)

### Updated
- `existing-doc.md` - What changed

## ✅ Quality Checks

- [x] **Build:** <Status and any notes>
- [x] **Tests:** <Test results>
- [x] **Coverage:** <Coverage percentage>
- [x] **Architecture:** <Follows documented patterns>
- [x] **Docs:** <Documentation complete>

## 🔄 Commit History

Brief summary of how commits are organized:
1. First logical unit
2. Second logical unit
3. etc.

## 🚀 Next Steps (Optional)

What future PRs this enables or what should come next.

---

**Related Issues:** #<issue-number> (if applicable)
**Breaking Changes:** Yes/No (explain if yes)

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

## Full Example

```markdown
# Knife Sharpening System with Pure Logic ECS Architecture

This PR implements a complete knife sharpening system using Arch ECS with a pure logic separation pattern that scales for future game mechanics.

## 🎯 Overview

Implemented a fully-featured knife sharpening mechanic with:
- **Pure business logic** (zero ECS dependencies)
- **Thin ECS systems** (orchestration only)
- **Command-Event bridge** for Godot integration
- **Feature-based architecture** ready to scale

## 🏗️ Architecture Pattern

### Pure Logic Separation
Following the pattern documented in `docs/architecture/ecs-logic-separation.md`:

```
✅ Pure Logic (SharpeningLogic.cs)
  - Static methods with no ECS dependencies
  - 100% testable without World management
  - Reusable across systems

✅ ECS System (SharpeningSystem.cs)
  - Thin wrapper calling pure logic
  - Handles queries and component updates only
  - Emits events for UI

✅ Command-Event Bridge (GameFacade.cs)
  - Godot → ECS via commands
  - ECS → Godot via events
  - Clean separation of concerns
```

### Benefits
- Tests run in **270ms** (no ECS overhead)
- **100% coverage** of business logic achievable
- No threading/memory issues in tests
- Logic is reusable and maintainable

## 📁 Directory Structure

Feature-based organization ready to scale:

```
apps/CookingProject.Logic/
├── Core/                        # Shared infrastructure
│   ├── Commands/IGameCommand.cs
│   ├── Events/IGameEvent.cs
│   └── Systems/IGameSystem.cs
├── Features/Sharpening/         # Self-contained feature
│   ├── Commands/
│   ├── Components/
│   ├── Events/
│   ├── Logic/SharpeningLogic.cs
│   └── SharpeningSystem.cs
└── GameFacade.cs
```

## 🎮 Game Design

### Sharpness Mechanic
- **Sharpness Level:** 0.0 (dull) → 1.0 (sharp)
- **Impact:** Affects chopping speed (3.33x slower when dull)
- **Degradation:** Decreases with use
- **Restoration:** Player sharpens at whetstone (5 seconds)

### Features
- ✅ Start/Cancel sharpening with progress tracking
- ✅ Upgradeable max sharpness (1.0 → 1.2 → 1.5)
- ✅ Faster sharpening stones (5s → 3s → 2s)
- ✅ Constant sharpening rate regardless of starting level
- ✅ Progress events for UI updates

## 🧪 Testing

### Test Coverage: 22 tests, 100% coverage, 270ms
```
✅ CalculateSharpenAmount (5 tests)
✅ ApplySharpeningProgress (4 tests)
✅ IsComplete (4 tests)
✅ CalculateProgressPercent (5 tests)
✅ Integration scenarios (4 tests)
```

### Sample Test
```csharp
[Fact]
public void Scenario_FullSharpening_From0To1_Over60Frames()
{
    float currentLevel = 0.0f;
    float deltaTime = 1.0f / 60.0f;

    for (int frame = 0; frame < 300; frame++)
    {
        float amount = SharpeningLogic.CalculateSharpenAmount(...);
        currentLevel = SharpeningLogic.ApplySharpeningProgress(...);
    }

    currentLevel.Should().BeApproximately(1.0f, 0.01f);
}
```

## 📦 Components

### Core Components (ECS)
- `Sharpness` - Tracks knife sharpness (Level, MaxLevel)
- `SharpeningProgress` - Active sharpening state
- `Tool` - Tag component for tools

### Commands (Godot → ECS)
- `StartSharpeningCommand` - Begin sharpening
- `CancelSharpeningCommand` - Cancel mid-sharpen

### Events (ECS → Godot)
- `SharpeningStartedEvent`
- `SharpeningProgressEvent`
- `KnifeSharpenedEvent`
- `SharpeningCancelledEvent`

## 📚 Documentation

### Added
- `docs/architecture/ecs-logic-separation.md` - **375 lines** of architecture guidance
- `docs/game-design/overview.md` - Knife sharpness game design
- `apps/CookingProject.Logic/README.md` - Command-Event pattern docs

### Key Sections
- ✅ Pattern overview with examples
- ✅ Testing strategy
- ✅ Implementation checklist
- ✅ Anti-patterns to avoid

## ✅ Quality Checks

- [x] **Build:** Success (0 errors)
- [x] **Tests:** 22/22 passing in 270ms
- [x] **Coverage:** 100% of pure logic
- [x] **Architecture:** Follows documented pattern
- [x] **Docs:** Comprehensive guides added
- [x] **Scalable:** Feature-based structure ready

## 🔄 Commit History

13 focused commits following best practices:
1. Architecture documentation
2. Game design documentation
3. Commands infrastructure
4. Events infrastructure
5. ECS Components
6. Pure business logic
...

## 🚀 Next Steps

This architecture enables easy addition of:
- Cooking mechanics (`Features/Cooking/`)
- Inventory system (`Features/Inventory/`)
- Recipe management (`Features/Recipes/`)

---

**Pattern Reference:** `docs/architecture/ecs-logic-separation.md`
**Tests:** `tests/CookingProject.Logic.Tests/Features/Sharpening/Logic/SharpeningLogicTests.cs`

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

## Tips

1. **Use emojis sparingly** - Only for section headers to improve scannability
2. **Include code snippets** - Show representative examples
3. **Link to documentation** - Reference related docs/patterns
4. **Show test results** - Actual numbers build confidence
5. **Explain "why"** - Don't just list changes, explain rationale
6. **Keep it scannable** - Use lists, headers, code blocks
7. **Quality checklist** - Shows thoroughness

## Anti-Patterns to Avoid

❌ **Too brief:**
```
Adds sharpening system

See commits for details.
```

❌ **Just a list of files:**
```
Modified:
- File1.cs
- File2.cs
- File3.cs
```

❌ **No testing info:**
```
Implements sharpening.

(Missing test coverage, results, examples)
```

✅ **Good PR:**
- Clear overview and context
- Architecture explanation
- Test coverage with results
- Documentation references
- Quality checklist
- Future direction

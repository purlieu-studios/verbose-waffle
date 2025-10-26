# Commit Message Template

Use this template for all commits to maintain consistency and clarity.

## Format

```
<Type>: <Brief summary in imperative mood>

<Detailed description of what changed and why>

<Optional sections as needed>

Build: ✅/❌ <build status>
Tests: ✅/❌ <test results>

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

## Types

- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation only
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **test**: Adding or updating tests
- **chore**: Maintenance tasks (deps, configs, etc.)
- **perf**: Performance improvement
- **style**: Code style changes (formatting, no logic change)

## Examples

### Feature Addition
```
feat: Add knife sharpening system with pure logic pattern

Implements complete sharpening mechanic:
- Pure business logic (SharpeningLogic.cs) with zero ECS dependencies
- Thin ECS system (SharpeningSystem.cs) for orchestration
- Command-Event bridge for Godot integration

Features:
- Start/Cancel sharpening with progress tracking
- Upgradeable max sharpness (1.0 → 1.2 → 1.5)
- Faster sharpening stones (5s → 3s)
- Constant rate regardless of starting level

Build: ✅ Success
Tests: ✅ 22/22 passing in 270ms

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Bug Fix
```
fix: Correct sharpening rate calculation

Fixed issue where sharpening slowed down as knife got sharper.

Problem: Used current sharpness level in rate calculation, causing
diminishing returns.

Solution:
- Track InitialLevel in SharpeningProgress component
- Use InitialLevel (not current) in CalculateSharpenAmount()
- Ensures constant sharpening rate

Build: ✅ Success
Tests: ✅ All passing (includes new regression test)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Refactor
```
refactor: Reorganize codebase into feature-based structure

New structure scales better for multiple game mechanics:

Before:
- Flat Commands/, Components/, Events/, Systems/ directories

After:
- Core/ (shared infrastructure)
- Features/Sharpening/ (self-contained feature)

Benefits:
- Clear separation: Core vs Features
- Easy to add new features (Cooking, Inventory, etc.)
- Related files grouped together
- Namespace alignment with directory structure

Build: ✅ Success
Tests: ✅ 22/22 passing

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Documentation
```
docs: Add ECS architecture pattern guide

Created comprehensive guide for pure logic separation pattern:
- Pattern overview with examples (375 lines)
- Testing strategy
- Implementation checklist
- Anti-patterns to avoid

File: docs/architecture/ecs-logic-separation.md

This establishes the standard for all future ECS systems.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

### Chore
```
chore: Update test project to target .NET 8.0

CI was failing because test project targeted net10.0 (preview)
but CI runners only have .NET SDK 9.0.

Changes:
- Update test project from net10.0 → net8.0
- Suppress CA1707 for test method underscores

Build: ✅ Success
Tests: ✅ 22/22 passing on CI

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

## Best Practices

1. **Subject line (first line):**
   - Max 50 characters
   - Start with type prefix
   - Use imperative mood ("Add" not "Added" or "Adds")
   - Don't end with period

2. **Body:**
   - Wrap at 72 characters
   - Explain **what** and **why**, not **how**
   - Include context for future maintainers

3. **Footer:**
   - Always include build/test status
   - Add Claude Code signature
   - Optional: Breaking changes, issue refs

4. **Keep commits focused:**
   - One logical change per commit
   - Under 300 lines when possible
   - Use multiple commits for large features

## Anti-Patterns to Avoid

❌ **Too vague:**
```
Update files
Fix bug
Changes
```

❌ **Too technical without context:**
```
Refactor CalculateSharpenAmount to use InitialLevel parameter
```

❌ **Missing status:**
```
Add sharpening system

(No build/test status)
```

✅ **Good:**
```
feat: Add sharpening system with pure logic pattern

Implements knife sharpening using documented ECS pattern.
All business logic testable without ECS overhead.

Build: ✅ Success
Tests: ✅ 22/22 passing

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

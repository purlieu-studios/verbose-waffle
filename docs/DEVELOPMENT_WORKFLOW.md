# Development Workflow

This document outlines the development workflow and quality gates for the cooking project monorepo.

## Quality Standards

### TL;DR - The Rules

1. **Small commits:** ≤300 LOC per commit
2. **All changes via PR:** No direct commits to `main` or `dev`
3. **All code tested:** 90%+ coverage required
4. **CI must pass:** All checks green before merge
5. **One logical change per commit:** Make it atomic and reviewable

## Workflow Overview

```
1. Create feature branch
   └─> 2. Make small, focused commits (<300 LOC)
        └─> 3. Write tests (maintain 90%+ coverage)
             └─> 4. Push branch
                  └─> 5. Create PR
                       └─> 6. CI validates everything
                            └─> 7. Review & merge
```

## Step-by-Step Guide

### 1. Start New Work

```bash
# Update main
git checkout main
git pull

# Create feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/bug-description
```

### 2. Make Small, Focused Commits

**Each commit should:**
- Represent ONE logical change
- Be ≤300 lines of code (added + removed)
- Have descriptive commit messages
- Include tests for new functionality

**Example - Good commits:**
```bash
# Commit 1: Add HealthComponent struct (50 LOC)
git add packages/game-logic/Components/HealthComponent.cs
git commit -m "Add HealthComponent with Max and Current fields"

# Commit 2: Add tests for HealthComponent (80 LOC)
git add packages/game-logic.Tests/Components/HealthComponentTests.cs
git commit -m "Add unit tests for HealthComponent"

# Commit 3: Add HealthSystem (120 LOC)
git add packages/game-logic/Systems/HealthSystem.cs
git commit -m "Add HealthSystem to process damage and healing"

# Commit 4: Add tests for HealthSystem (150 LOC)
git add packages/game-logic.Tests/Systems/HealthSystemTests.cs
git commit -m "Add unit tests for HealthSystem"
```

**Example - Bad commit:**
```bash
# ❌ Too large, multiple changes
git add .
git commit -m "Add health system"  # 600 LOC, mixes features, no focus
```

### 3. Write Tests First (TDD Recommended)

**Test-Driven Development cycle:**
```
1. Write failing test
2. Write minimal code to pass
3. Refactor
4. Repeat
```

**Coverage requirements:**
- **90% minimum** for all code
- **100% target** for critical paths (ECS components/systems, recipe logic)
- **Tests required** for all public APIs

**Test structure:**
```
cooking-project/
├── packages/
│   ├── game-logic/
│   │   └── Components/
│   │       └── HealthComponent.cs
│   └── game-logic.Tests/          # Tests mirror structure
│       └── Components/
│           └── HealthComponentTests.cs
```

### 4. Local Pre-Commit Checks

**Git hooks automatically check:**
- ✅ Commit size (≤300 LOC)
- ✅ Protected files not modified
- ✅ Code quality rules intact

**Hook will block:**
```bash
# ❌ Commit too large
$ git commit -m "Big feature"
BLOCKED: Commit Too Large
This commit changes 450 lines (limit: 300).
```

**To split commits:**
```bash
# Interactive staging
git add -p

# Stage specific files
git add path/to/file1.cs
git commit -m "Focused change 1"

git add path/to/file2.cs
git commit -m "Focused change 2"
```

### 5. Push and Create PR

```bash
# Push branch
git push -u origin feature/your-feature-name

# Create PR
gh pr create \
  --title "Add health system for player damage" \
  --body "Implements health tracking with components and systems"
```

### 6. CI Validation (Automatic)

**When you create a PR, CI automatically runs:**

#### PR Validation (`pr-validation.yml`)
- ✅ Checks each commit size (≤300 LOC)
- ✅ Checks total PR size (≤1000 LOC recommended)
- ✅ Warns if no tests added with code changes

#### .NET CI (`dotnet-ci.yml`)
- ✅ Builds all C# projects
- ✅ Runs all tests
- ✅ Checks coverage (≥90%)
- ✅ Runs code analyzers (treat warnings as errors)
- 📊 Posts coverage report as PR comment

#### Python CI (`python-ci.yml`)
- ✅ Runs MCP server tests
- ✅ Checks coverage (≥90%)
- ✅ Runs linters (Black, isort, Flake8)
- ✅ Type checks (MyPy)

#### Rule Validation (`validate-rules.yml`)
- ✅ Ensures quality rules not weakened
- ✅ Checks if CODE_QUALITY_RULES.md updated

**All checks must pass** before PR can be merged!

### 7. Code Review & Merge

**Review checklist:**
- [ ] All CI checks green
- [ ] Code coverage ≥90%
- [ ] Each commit is focused and atomic
- [ ] Tests cover new functionality
- [ ] No code quality rules weakened
- [ ] Documentation updated if needed

**Merge:**
```bash
# Via GitHub UI (recommended)
# Click "Squash and merge" or "Merge pull request"

# Or via CLI
gh pr merge --squash  # Combines commits
gh pr merge --merge   # Keeps commit history
```

## Quality Gates Summary

### Commit Level
- **Size:** ≤300 LOC per commit
- **Focus:** One logical change
- **Tested:** Include tests in same PR

### PR Level
- **Size:** ≤1000 LOC recommended
- **Tests:** Coverage ≥90%
- **CI:** All checks must pass
- **Review:** At least 1 approval (can self-approve as maintainer)

### Repository Level
- **No direct commits** to `main` or `dev`
- **Branch protection** enforced
- **Quality rules** protected from weakening

## Common Scenarios

### "My commit is 350 LOC, just slightly over"

**Don't bypass!** Split it:
```bash
# Reset the commit
git reset HEAD~1

# Stage and commit in chunks
git add -p  # Interactive staging
git commit -m "Part 1: Add component definitions"

git add -p
git commit -m "Part 2: Add system logic"
```

### "I need to make an emergency hotfix"

**Still use PRs**, but you can fast-track:
```bash
# Create fix branch
git checkout -b hotfix/critical-bug

# Make fix (keep it small!)
git commit -m "Fix null reference in RecipeSystem"

# Push and create PR
gh pr create --title "[HOTFIX] Fix critical null reference"

# As maintainer, you can approve your own PR
gh pr merge --squash
```

### "CI is failing on coverage"

**Write more tests:**
```bash
# Find uncovered code
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
open coverage/index.html  # Shows what's not covered

# Add tests for uncovered paths
# Push again - CI will re-run
```

### "I forgot to add tests"

**Add them before requesting review:**
```bash
# Add test files
git add packages/game-logic.Tests/NewFeatureTests.cs
git commit -m "Add unit tests for new feature"

# Push - CI will re-run with new coverage
git push
```

## Tools & Commands

### Check commit size before committing
```bash
# See what would be committed
git diff --cached --numstat

# Count total lines
git diff --cached --numstat | awk '{sum+=$1+$2} END {print sum}'
```

### Run tests locally
```bash
# .NET tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Python tests with coverage
pytest tools/ --cov=tools --cov-report=term
```

### Bypass hooks (emergency only)
```bash
# Skip pre-commit checks
git commit --no-verify

# ⚠️ Use sparingly! Document why in commit message
```

## CI Configuration Files

- `.github/workflows/pr-validation.yml` - PR size and quality checks
- `.github/workflows/dotnet-ci.yml` - .NET build, test, coverage
- `.github/workflows/python-ci.yml` - Python MCP server validation
- `.github/workflows/validate-rules.yml` - Code quality rule protection

## Git Hooks

- `.githooks/pre-commit` - Blocks protected file changes
- `.githooks/commit-msg` - Enforces commit size limits

### Enable hooks
```bash
git config core.hooksPath .githooks
```

## Best Practices

### Commit Messages
```bash
# Good
git commit -m "Add HealthComponent with Max and Current properties"
git commit -m "Fix off-by-one error in inventory calculation"
git commit -m "Refactor RecipeSystem to use query pattern"

# Bad
git commit -m "stuff"
git commit -m "WIP"
git commit -m "Fixed things"
```

### PR Titles
```bash
# Good
"Add health system for player damage tracking"
"Fix recipe scoring calculation bug"
"Refactor: Extract ingredient validation to separate system"

# Bad
"Updates"
"Changes"
"PR"
```

### Test Organization
```csharp
// Good - one test, one assertion focus
[Fact]
public void HealthComponent_WhenDamaged_ReducesCurrentHealth()
{
    var health = new HealthComponent { Max = 100, Current = 100 };
    health.Current -= 20;
    Assert.Equal(80, health.Current);
}

[Fact]
public void HealthComponent_WhenHealed_DoesNotExceedMax()
{
    var health = new HealthComponent { Max = 100, Current = 80 };
    health.Current += 30;
    Assert.Equal(100, health.Current);
}
```

## Troubleshooting

### Hook not running
```bash
# Check hooks path
git config core.hooksPath
# Should output: .githooks

# Fix it
git config core.hooksPath .githooks
```

### CI failing but works locally
```bash
# Make sure you're testing the same way CI does
dotnet test --configuration Release
pytest tools/ --cov=tools --cov-fail-under=90
```

### Can't merge - coverage too low
```bash
# Find gaps in coverage
dotnet test --collect:"XPlat Code Coverage"
# Add tests for uncovered code
# Push again
```

## Philosophy

> **"Small commits, high quality, always tested"**

These rules exist to:
- **Catch bugs early** before they reach production
- **Make reviews effective** - small PRs are actually reviewed
- **Maintain quality** - 90% coverage ensures code is testable
- **Enable confidence** - green CI means safe to deploy
- **Create good history** - atomic commits help with bisecting and reverting

**The friction is intentional.** It's easier to write quality code than to fix bugs in production.

## See Also

- [Code Quality Rules](CODE_QUALITY_RULES.md) - Why rules exist, how to change them
- [Branch Protection](../.github/BRANCH_PROTECTION.md) - GitHub protection setup
- [Working with Claude](../CLAUDE.md) - AI-assisted development tips

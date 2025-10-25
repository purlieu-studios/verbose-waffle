# Code Quality Rules

## Purpose

This document explains why strict code quality rules exist and the process for changing them.

## Why These Rules Exist

**The rules are intentionally strict.** They exist to:

1. **Prevent security vulnerabilities** - Security issues become errors, not suggestions
2. **Catch bugs early** - Many runtime bugs are caught at compile time
3. **Maintain consistency** - All code follows the same patterns and style
4. **Improve performance** - Performance anti-patterns are flagged automatically
5. **Enable safe refactoring** - Nullable reference types and strict typing make refactoring safer
6. **Reduce technical debt** - Issues must be fixed immediately, not deferred

## The Temptation to Weaken Rules

When facing a difficult analyzer error, you may be tempted to:
- Change a rule from `Error` to `Warning` or `None`
- Disable `TreatWarningsAsErrors`
- Add suppressions or disable analyzers
- Modify `.editorconfig` to relax standards

**STOP.** Ask yourself:

### Is the rule catching a real problem?

**YES** → Fix the code, not the rule.

Most analyzer errors indicate:
- Actual bugs (null references, dispose issues, async problems)
- Security vulnerabilities (SQL injection, weak crypto)
- Performance problems (unnecessary allocations, inefficient patterns)
- Maintainability issues (complexity, poor naming)

### Is the rule a false positive?

**Rare, but possible.** Before changing the rule:

1. **Research the rule** - Understand WHY it exists (search for the CA/SA number)
2. **Get a second opinion** - Have another developer review the code
3. **Use targeted suppression** - If truly a false positive, suppress ONLY that specific instance with justification:
   ```csharp
   #pragma warning disable CA1234 // Reason: [Detailed explanation of why this is safe]
   // ... code ...
   #pragma warning restore CA1234
   ```

### The code is "too hard to fix"?

**This is not a valid reason.**

Hard-to-fix code quality issues usually indicate:
- Poor architecture that needs refactoring
- Missing abstractions
- Over-complicated logic

The difficulty of fixing is a signal that the code needs improvement, not that the rule is wrong.

## Process for Changing Rules

If you genuinely need to change a code quality rule (rare), follow this process:

### 1. Document the Reason

Create an issue or RFC explaining:
- Which rule(s) you want to change
- Why the rule is problematic in this codebase
- What specific scenarios cause issues
- Why fixing the code isn't feasible
- What risk mitigation you'll put in place

### 2. Get Approval

Changes to these files require approval from `@purlieu-studios/maintainers`:
- `CodeAnalysis.ruleset`
- `Directory.Build.props`
- `.editorconfig`

This is enforced by:
- `.github/CODEOWNERS`
- Pre-commit hooks (`.githooks/pre-commit`)
- CI validation (`.github/workflows/validate-rules.yml`)

### 3. Update This Document

Add an entry to the "Rule Changes" section below documenting:
- Date of change
- Rule(s) modified
- Rationale
- Who approved

### 4. Make the Change

Only after approval and documentation:
```bash
# You'll need to bypass the pre-commit hook
git commit --no-verify -m "Relax rule CA1234: [detailed reason]"
```

## Rule Changes

### Initial Configuration - 2024-10-24

**Configured by:** Initial setup
**Rationale:** Establish baseline strict enforcement for security, performance, quality, and style.
**All rules set to `Error` except:**
- `CA1031` (Generic catch) - `Warning` - Sometimes generic catch is necessary for top-level handlers
- `CA1502` (Cyclomatic complexity) - `Warning` - Allow some flexibility for complex domains
- `CA1506` (Class coupling) - `Warning` - Allow some flexibility for integration code
- `SA1309` (Field naming) - `None` - Allow `_camelCase` for private fields (common C# pattern)
- `SA1600-SA1633` (Documentation) - `None` - XML docs not required for all code

---

## Common Scenarios and Solutions

### "The null check is annoying"

**Rule:** `CS8602`, `CA1062` - Nullable reference types

**Why it exists:** Null references are the #1 cause of runtime crashes.

**Solution:**
```csharp
// Bad - will trigger warning
public void Process(string input)
{
    Console.WriteLine(input.Length); // CS8602
}

// Good - handle null case
public void Process(string? input)
{
    if (input == null)
        throw new ArgumentNullException(nameof(input));

    Console.WriteLine(input.Length);
}

// Also good - express intent with non-nullable parameter
public void Process(string input)
{
    ArgumentNullException.ThrowIfNull(input);
    Console.WriteLine(input.Length);
}
```

### "ConfigureAwait is tedious"

**Rule:** `CA2007` - Use ConfigureAwait

**Why it exists:** Prevents deadlocks and improves performance in libraries.

**Solution:**
```csharp
// Bad
await SomeAsyncMethod();

// Good - in library/framework code
await SomeAsyncMethod().ConfigureAwait(false);

// Good - in UI/ASP.NET code where you need context
await SomeAsyncMethod().ConfigureAwait(true);
```

### "The complexity rule is blocking me"

**Rule:** `CA1502` - Cyclomatic complexity too high

**Why it exists:** Complex methods are hard to test, maintain, and often contain bugs.

**Solution:** Extract methods, use guard clauses, simplify conditionals.

```csharp
// Bad - high complexity
public void ProcessOrder(Order order)
{
    if (order != null)
    {
        if (order.Items.Count > 0)
        {
            if (order.Customer != null)
            {
                // ... many nested conditions
            }
        }
    }
}

// Good - guard clauses and extracted methods
public void ProcessOrder(Order order)
{
    ArgumentNullException.ThrowIfNull(order);
    if (order.Items.Count == 0) return;
    ArgumentNullException.ThrowIfNull(order.Customer);

    ValidateOrder(order);
    CalculateTotals(order);
    ProcessPayment(order);
}
```

### "I need to use reflection/dynamic"

Some analyzers don't understand reflection. Use targeted suppression:

```csharp
#pragma warning disable CA1062 // Validate arguments - false positive with reflection
var value = propertyInfo.GetValue(obj);
#pragma warning restore CA1062
```

## Remember

> "The rules are your friend, not your enemy. They catch bugs before your users do."

When in doubt: **Fix the code, not the rules.**

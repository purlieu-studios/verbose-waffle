# Git Hooks

This directory contains git hooks that protect code quality standards.

## Setup

After cloning this repository, run:

```bash
git config core.hooksPath .githooks
```

This configures git to use the hooks in this directory instead of `.git/hooks`.

## Hooks

### pre-commit

Prevents commits that modify code quality configuration files:
- `CodeAnalysis.ruleset`
- `Directory.Build.props`
- `.editorconfig`

**Why?** These files enforce code quality standards. Changes should be rare and deliberate, not casual workarounds for fixing legitimate code issues.

**To bypass** (emergency only):
```bash
git commit --no-verify
```

Before bypassing, read `CODE_QUALITY_RULES.md` for the proper change process.

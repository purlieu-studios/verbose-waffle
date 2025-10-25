# Cooking Project

A monorepo for a cooking-themed game with shared game logic and web services.

## Project Structure

```
cooking-project/
├── apps/                    # Runnable applications
│   ├── game/               # Godot game project
│   └── web/                # ASP.NET backend/API
│
├── packages/               # Shared libraries/packages
│   └── game-logic/        # C# Game Class Library (shared logic)
│
├── tools/                  # Development tools
│   └── code-search/       # MCP server for semantic code search
│
├── docs/                   # Project-wide documentation
│   ├── CLAUDE.md          # Working with Claude Code
│   └── CODE_QUALITY_RULES.md
│
├── scripts/                # Build/CI/setup scripts
├── .github/                # GitHub workflows & templates
└── .githooks/              # Git hooks
```

## Getting Started

### Prerequisites

- .NET SDK 8.0+
- Godot Engine 4.x
- Python 3.10+ (for code-search tool)

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/purlieu-studios/verbose-waffle.git
   cd cooking-project
   ```

2. Set up the code-search tool (optional but recommended):
   - Windows: `.\tools\code-search\setup_rag.bat`
   - macOS/Linux: `./tools/code-search/setup_rag.sh`

3. Open the Godot project:
   - Navigate to `apps/game/`
   - Open `project.godot` in Godot Engine

4. Build the solution:
   ```bash
   dotnet build CookingProject.sln
   ```

## Documentation

- **[Development Workflow](docs/DEVELOPMENT_WORKFLOW.md)** - Start here! Complete guide to our development process
- **[Working with Claude Code](CLAUDE.md)** - How to use Claude Code in this project
- [Code Quality Rules](docs/CODE_QUALITY_RULES.md)
- [Code Search Tool](tools/code-search/README.md)

## Development

This is a monorepo containing multiple projects that work together:

- **Game** (Godot): The main game client (`apps/game/`)
  - Godot-specific code and scene scripts
- **Game Logic** (C# Library): Shared game logic (`packages/game-logic/`)
  - Core game systems, ECS components, recipe logic
  - Strict code quality enforcement (analyzers, tests required)
  - Shared between game client and web backend
- **Web** (ASP.NET): Backend services, APIs, player accounts (`apps/web/`)
  - REST APIs, authentication, leaderboards
  - Strict code quality enforcement

**Where to put code:**
- Godot-specific UI/scene code → `apps/game/`
- Shared game logic (ECS, recipes, etc.) → `packages/game-logic/`
- Backend/API code → `apps/web/`

### Quality Standards

We enforce strict quality standards to ensure maintainable, bug-free code:

#### 🔒 Enforced by Git Hooks
- **Small commits:** ≤300 LOC per commit
- **Protected configs:** Code quality rules can't be weakened accidentally

#### 🤖 Enforced by CI
- **Test coverage:** 90%+ required on all code
- **All tests pass:** No failing tests allowed
- **Code analysis:** Warnings treated as errors
- **PR size:** ≤1000 LOC recommended per PR

#### 📋 Development Workflow
1. Create feature branch
2. Make small, focused commits (<300 LOC each)
3. Write tests (maintain 90%+ coverage)
4. Create pull request
5. CI validates everything automatically
6. Review and merge when green

**See [DEVELOPMENT_WORKFLOW.md](docs/DEVELOPMENT_WORKFLOW.md) for complete details.**

### Quick Start for Contributors

```bash
# Enable git hooks
git config core.hooksPath .githooks

# Create feature branch
git checkout -b feature/your-feature

# Make small commits with tests
git commit -m "Add HealthComponent"  # <300 LOC
git commit -m "Add tests for HealthComponent"  # <300 LOC

# Push and create PR
git push -u origin feature/your-feature
gh pr create

# CI will validate:
# ✅ Commit sizes
# ✅ Test coverage (90%+)
# ✅ All tests pass
# ✅ Code quality rules
```

## Contributing

**Before contributing, read:**
1. [Development Workflow](docs/DEVELOPMENT_WORKFLOW.md) - Required reading
2. [Code Quality Rules](docs/CODE_QUALITY_RULES.md) - Why the rules exist

**TL;DR:**
- Small commits (<300 LOC)
- Always include tests (90%+ coverage)
- All PRs must pass CI
- One logical change per commit

## License

TBD

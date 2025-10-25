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

- [Working with Claude Code](docs/CLAUDE.md)
- [Code Quality Rules](docs/CODE_QUALITY_RULES.md)
- [Code Search Tool](tools/code-search/README.md)

## Development

This is a monorepo containing multiple projects that work together:

- **Game** (Godot): The main game client
- **Game Logic** (C# Library): Shared game logic used by both game and web
- **Web** (ASP.NET): Backend services, APIs, and player accounts

## Contributing

See [CODE_QUALITY_RULES.md](docs/CODE_QUALITY_RULES.md) for coding standards and practices.

## License

TBD

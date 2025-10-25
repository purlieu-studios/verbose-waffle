# Future MCP Server Ideas

This document tracks potential MCP servers to build for the cooking game project.

## Custom MCP Servers to Build

### 1. Arch ECS MCP Server ⭐ HIGH VALUE!
**Purpose:** Streamline development with Arch ECS in Godot + C#

**Why Build This:**
- No existing ECS MCP servers exist
- Godot MCP plugins only support GDScript
- Would benefit entire Arch ECS + Godot community
- Great open-source portfolio piece
- Solves OUR immediate development needs

**Potential Tools:**
- `create_component(name: str, fields: dict)` - Generate component structs
- `create_system(name: str, query: list[str])` - Generate system boilerplate
- `create_entity_archetype(name: str, components: list[str])` - Generate common entity types
- `find_entities_with_components(components: list[str])` - Search codebase for entities
- `analyze_query_performance(system_name: str)` - Performance optimization suggestions
- `list_all_components()` - Inventory all components in project
- `list_all_systems()` - Inventory all systems
- `validate_component_usage(component_name: str)` - Find unused components
- `generate_component_documentation()` - Auto-document ECS architecture
- `suggest_system_ordering()` - Recommend system execution order

**Implementation Approach:**
- Use FastMCP (like our code-search server)
- Parse C# files to understand component/system structure
- Use Roslyn (C# compiler API) for robust C# parsing
- Integration with code-search for intelligent suggestions

**Benefits:**
- Rapid ECS development ("create a Health component with Max and Current fields")
- Architecture validation and optimization
- Documentation generation
- Community contribution opportunity

**Timeline:** Build after ECS architecture is established in game (Month 2-3?)

---

### 2. Game Data Server
**Purpose:** Manage recipes, ingredients, and game balancing

**Potential Tools:**
- `get_recipe(recipe_name: str)` - Fetch recipe configurations
- `list_all_recipes()` - Get all available recipes
- `validate_ingredient_combo(ingredients: list[str])` - Check if ingredients work together
- `calculate_recipe_score(ingredients: list[str], cooking_method: str)` - Score a player's dish
- `suggest_ingredient_prices()` - AI-powered economic balancing
- `analyze_recipe_difficulty(recipe_id: str)` - Difficulty analysis
- `get_flavor_profile(ingredients: list[str])` - Determine flavor combinations

**Data Sources:**
- JSON/YAML recipe database
- Ingredient properties database
- Game balance configuration files

**Benefits:**
- Claude can help balance recipes without manual file editing
- Quick recipe lookups during development
- AI-powered suggestions for new content
- Validate game economy

---

### 2. Godot Scene Inspector
**Purpose:** Parse and analyze Godot `.tscn` and `.gd` files

**Potential Tools:**
- `get_scene_structure(scene_path: str)` - Parse scene hierarchy
- `find_node(scene_path: str, node_name: str)` - Locate specific nodes
- `list_scene_scripts(scene_path: str)` - Get all attached scripts
- `find_unused_scenes()` - Identify orphaned scene files
- `validate_scene_references()` - Check for broken references
- `get_node_properties(scene_path: str, node_path: str)` - Extract node configuration
- `find_scenes_using_script(script_path: str)` - Reverse lookup

**Benefits:**
- Understand complex scene hierarchies
- Find and fix broken references
- Refactor scenes with AI assistance
- Documentation generation
- Asset cleanup

---

## Third-Party MCP Servers to Consider

### Game Development (Godot-Specific!)

**GDAI MCP Plugin** ⭐ HIGHLY RECOMMENDED
- **URL:** https://gdaimcp.com/
- **GitHub:** https://github.com/ee0pdt/Godot-MCP
- **Capabilities:**
  - Control Godot Editor directly from Claude
  - Create scenes, resources, and scripts
  - Read and fix errors automatically
  - Run the game and verify output with screenshots
  - Scene manipulation and project management
- **Use Case:** Let Claude help build your game scenes, fix errors, and test gameplay

**Godot MCP Server by Coding-Solo**
- **GitHub:** https://github.com/Coding-Solo/godot-mcp
- **Capabilities:**
  - Launch Godot editor
  - Run projects
  - Capture debug output
  - Control project execution
- **Use Case:** Project automation and debugging

### Database

**PostgreSQL MCP** (Official Anthropic)
- Schema inspection
- Read-only database access
- Perfect for ASP.NET backend

**SQLite MCP** (Official Anthropic)
- Lightweight local database
- Great for development/testing

**MongoDB MCP** (Official MongoDB - May 2025)
- NoSQL database access
- Public preview as of May 2025
- Integrates with Claude, Cursor, VS Code

**mcp-dbs** (Unified Multi-Database)
- **Install:** `npm install -g mcp-dbs`
- Supports: SQLite, PostgreSQL, SQL Server, MongoDB
- Single server for multiple database types

### Web Development & Scraping

**Firecrawl MCP**
- Powerful web scraping and search
- Could scrape real cooking recipes for game inspiration!

**Playwright**
- Browser automation
- Perfect for testing your ASP.NET web app

**Chrome DevTools MCP** (Official)
- Control and inspect live Chrome browser
- Advanced debugging

**Browserbase**
- Cloud-based browser automation
- Navigation and data extraction

### Cloud & Deployment

**Supabase**
- Database, authentication, edge functions
- Could power your player accounts system

**Cloudflare**
- Deploy Workers, KV storage, R2, D1 databases
- Hosting for your web app

### Development Tools

**Next.js DevTools MCP** (Official)
- If you ever switch to Next.js for the web frontend

**E2B**
- Run code in secure sandboxes
- Safe execution environment

### AI & Search

**Exa**
- Search engine built for AI
- Enhanced search capabilities

**MiniMax MCP**
- Text-to-speech generation
- Image and video generation
- Could create voice lines for NPCs!

### Unique/Creative

**Anki MCP**
- Spaced-repetition flashcards
- Could help players learn recipes?

### Resources
- MCP Directory: https://mcpservers.org/
- Official Servers: https://github.com/modelcontextprotocol/servers
- PulseMCP Search: https://www.pulsemcp.com/servers

---

## Implementation Priority

### Already Implemented
1. ✅ **Code Search** - Working perfectly!

### High Priority (Next Steps)
2. 🎮 **GDAI MCP Plugin** - Install ASAP! This will revolutionize Godot development
   - Claude can create scenes, fix errors, run tests
   - Eliminates context switching between editor and Claude
   - NOTE: GDScript only - won't understand our C# Arch ECS code

3. 🏗️ **Arch ECS MCP Server** (Custom) - Build once ECS architecture is established
   - Fill the gap left by GDScript-only Godot MCP
   - Community contribution opportunity
   - High impact for our C# workflow

4. 🍳 **Game Data Server** (Custom) - Build when recipe system is designed
   - Recipe management
   - Ingredient balancing
   - AI-powered game design assistance

### Medium Priority (When Needed)
5. 🗄️ **Database MCP** - When ASP.NET backend starts
   - Consider PostgreSQL or SQLite
   - Or use mcp-dbs for multi-database support

6. 🌐 **Firecrawl MCP** - For content research
   - Scrape real recipes for inspiration
   - Research cooking techniques

### Low Priority (Future Enhancements)
7. 🎭 **Playwright** - Web app testing automation
8. ☁️ **Supabase/Cloudflare** - When deploying to production
9. 🎤 **MiniMax MCP** - If adding voice acting to NPCs

### Maybe Someday
- Anki MCP for player tutorial system
- E2B for community-submitted recipe testing

---

## Resources

- FastMCP Documentation: https://github.com/jlowin/fastmcp
- MCP Specification: https://spec.modelcontextprotocol.io/
- Community Servers: https://mcpservers.org/
